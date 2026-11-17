using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardPurchasing;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardPurchasing;

public class GiftCardPurchasingCommandTests : BaseIntegrationTest
{
    public GiftCardPurchasingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Gift_card_purchase_debits_the_wallet_and_records_the_card_and_transaction()
    {
        var walletSeed = WalletSeed.GiftCardBuyerWallet;
        var senderId = walletSeed.TouristId;
        var client = Factory.CreateClientFor(senderId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCount = arrangeContext.GiftCards.Count();
        var walletBalance = walletSeed.Balance;
        var transactionCount = walletSeed.Transactions.Count;

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("EXPLORER", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var giftCardId = await response.Content.ReadFromJsonAsync<Guid>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.TouristId == senderId);
        var storedCard = assertContext.GiftCards.Single(giftCard => giftCard.Id == giftCardId);
        assertContext.GiftCards.Count().Should().Be(giftCardCount + 1);
        storedWallet.Balance.Should().Be(walletBalance - 75);
        storedWallet.Transactions.Should().HaveCount(transactionCount + 1);
        storedWallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.GiftCardPurchase
            && transaction.GiftCardId == giftCardId
            && transaction.Amount.Value == 75
            && transaction.EntityId == null
            && transaction.Name == null);
        storedCard.SenderId.Should().Be(senderId);
        storedCard.RecipientId.Should().Be(WellKnownUsers.Explorer);
        storedCard.Amount.Value.Should().Be(75);
        storedCard.Message.Should().Be("Enjoy the trip");
        storedCard.ExpiresAt.Should().Be(storedCard.CreatedAt.AddYears(3));
        storedCard.GetStatus(storedCard.CreatedAt).Should().Be(GiftCardStatus.Valid);
    }

    [Fact]
    public async Task A_partial_username_match_is_rejected_without_creating_a_card_or_changing_the_wallet()
    {
        var walletSeed = WalletSeed.GiftCardBuyerWallet;
        var senderId = walletSeed.TouristId;
        var client = Factory.CreateClientFor(senderId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCount = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("explor", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.TouristId == senderId);
        storedWallet.Balance.Should().Be(walletSeed.Balance);
        storedWallet.Transactions.Should().HaveSameCount(walletSeed.Transactions);
        assertContext.GiftCards.Count().Should().Be(giftCardCount);
    }

    [Fact]
    public async Task A_gift_card_cannot_be_sent_to_the_sender_without_changing_the_wallet()
    {
        var walletSeed = WalletSeed.ExplorersEmptyWallet;
        var senderId = WellKnownUsers.Explorer;
        var client = Factory.CreateClientFor(senderId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCount = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("EXPLORER", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Single(wallet => wallet.TouristId == senderId).Balance.Should().Be(walletSeed.Balance);
        assertContext.Wallets.Single(wallet => wallet.TouristId == senderId).Transactions.Should().HaveSameCount(walletSeed.Transactions);
        assertContext.GiftCards.Count().Should().Be(giftCardCount);
    }

    [Fact]
    public async Task A_gift_card_purchase_without_enough_EC_is_rejected_without_creating_a_card_or_changing_the_wallet()
    {
        var walletSeed = WalletSeed.ExplorersEmptyWallet;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var giftCardCount = arrangeContext.GiftCards.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("secondexplorer", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.TouristId == WellKnownUsers.Explorer);
        storedWallet.Balance.Should().Be(walletSeed.Balance);
        storedWallet.Transactions.Should().HaveSameCount(walletSeed.Transactions);
        assertContext.GiftCards.Count().Should().Be(giftCardCount);
    }

    [Fact]
    public async Task An_anonymous_user_cannot_purchase_a_gift_card()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("secondexplorer", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_administrator_cannot_purchase_a_gift_card()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards",
            new PurchaseGiftCardDto("secondexplorer", 75, "Enjoy the trip"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
