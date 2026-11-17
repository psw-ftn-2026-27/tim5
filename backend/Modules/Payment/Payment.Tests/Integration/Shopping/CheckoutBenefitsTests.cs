using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Shopping;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Shopping;

public class CheckoutBenefitsTests : BaseIntegrationTest
{
    public CheckoutBenefitsTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_previews_a_coupon_and_gift_card_without_changing_saved_state()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var card = GiftCardSeed.CheckoutCardForBuyer;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCount = arrangeContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Count;

        var preview = await client.GetFromJsonAsync<CheckoutPreviewDto>(
            $"/api/payment/cart/checkout-preview?couponCode={CouponSeed.CartWideTwentyPercentCoupon.Code}&giftCardId={card.Id}",
            JsonOptions);

        preview!.CartTotal.Should().Be(350);
        preview.CouponDiscount.Should().Be(40);
        preview.GiftCardDiscount.Should().Be(100);
        preview.Total.Should().Be(210);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Single(item => item.Id == wallet.Id).Balance.Should().Be(wallet.Balance);
        assertContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Should().HaveCount(transactionCount);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().BeNull();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().HaveCount(cart.Items.Count);
    }

    [Fact]
    public async Task Buyer_checks_out_with_a_coupon_and_gift_card_and_pays_only_the_remaining_amount()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var card = GiftCardSeed.CheckoutCardForBuyer;
        var coupon = CouponSeed.CartWideTwentyPercentCoupon;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto(coupon.Code, card.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = (await response.Content.ReadFromJsonAsync<Guid>(JsonOptions));
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance - 210);
        storedWallet.Transactions.Where(item => item.Type == TransactionType.Purchase)
            .Sum(item => item.Amount.Value).Should().Be(210);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().NotBeNull();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().BeEmpty();
        assertContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId).Should().Be(cart.Items.Count);
        var purchase = assertContext.Purchases.Single(item => item.Id == purchaseId);
        purchase.Total.Should().Be(210);
        purchase.CouponId.Should().Be(coupon.Id);
        purchase.CouponDiscount.Should().Be(40);
        purchase.GiftCardId.Should().Be(card.Id);
        purchase.GiftCardDiscount.Should().Be(100);
    }

    [Fact]
    public async Task Buyer_checks_out_for_zero_EC_without_a_wallet_transaction()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var card = GiftCardSeed.OversizedCheckoutCardForBuyer;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionsBefore = arrangeContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Count;
        var tokensBefore = arrangeContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId);

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto(CouponSeed.CartWideFullDiscountCoupon.Code, card.Id));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance);
        storedWallet.Transactions.Should().HaveCount(transactionsBefore);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().NotBeNull();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().BeEmpty();
        assertContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId).Should().Be(tokensBefore + cart.Items.Count);
        assertContext.Purchases.Single(item => item.Id == purchaseId).Total.Should().Be(0);
    }

    [Fact]
    public async Task Checkout_rejects_an_invalid_active_coupon_and_preserves_the_cart_and_wallet()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var purchasesBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto("NOCOUPON", null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().HaveCount(cart.Items.Count);
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance);
        assertContext.Purchases.Should().HaveCount(purchasesBefore);
        assertContext.GiftCards.Single(item => item.Id == GiftCardSeed.CheckoutCardForBuyer.Id).RedeemedAt.Should().BeNull();
    }

    [Fact]
    public async Task Checkout_rejects_an_expired_gift_card_without_partially_changing_purchase_state()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var card = GiftCardSeed.ExpiredCheckoutCardForBuyer;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionsBefore = arrangeContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Count;
        var tokensBefore = arrangeContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId);
        var purchasesBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto(null, card.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().HaveCount(cart.Items.Count);
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance);
        storedWallet.Transactions.Should().HaveCount(transactionsBefore);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().BeNull();
        assertContext.Purchases.Should().HaveCount(purchasesBefore);
        assertContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId).Should().Be(tokensBefore);
    }

    [Fact]
    public async Task Checkout_rejects_another_users_gift_card_without_changing_purchase_state()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var wallet = WalletSeed.BuyersWalletWithEnoughEC;
        var card = GiftCardSeed.CheckoutCardForAnotherBuyer;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionsBefore = arrangeContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Count;
        var tokensBefore = arrangeContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId);
        var purchasesBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto(null, card.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items.Should().HaveCount(cart.Items.Count);
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance);
        storedWallet.Transactions.Should().HaveCount(transactionsBefore);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().BeNull();
        assertContext.Purchases.Should().HaveCount(purchasesBefore);
        assertContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId).Should().Be(tokensBefore);
    }

    [Fact]
    public async Task Checkout_rejects_an_archived_tour_with_coupon_and_gift_card_without_changing_purchase_state()
    {
        var cart = ShoppingCartSeed.BuyersCartWithArchivedTour;
        var wallet = WalletSeed.BuyersWalletWithArchivedTourInCart;
        var card = GiftCardSeed.CheckoutCardForArchivedCartBuyer;
        var coupon = CouponSeed.CartWideTwentyPercentCoupon;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionsBefore = arrangeContext.Wallets.Single(item => item.Id == wallet.Id).Transactions.Count;
        var tokensBefore = arrangeContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId);
        var purchasesBefore = arrangeContext.Purchases.Count();

        var response = await client.PostAsJsonAsync(
            "/api/payment/cart/checkout",
            new CheckoutBenefitsDto(coupon.Code, card.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("These tours are no longer available");
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(item => item.Id == cart.Id).Items
            .Select(item => item.EntityId)
            .Should().BeEquivalentTo(cart.Items.Select(item => item.EntityId));
        var storedWallet = assertContext.Wallets.Single(item => item.Id == wallet.Id);
        storedWallet.Balance.Should().Be(wallet.Balance);
        storedWallet.Transactions.Should().HaveCount(transactionsBefore);
        assertContext.GiftCards.Single(item => item.Id == card.Id).RedeemedAt.Should().BeNull();
        assertContext.Purchases.Should().HaveCount(purchasesBefore);
        assertContext.TourPurchaseTokens.Count(item => item.TouristId == cart.TouristId).Should().Be(tokensBefore);
    }

    [Fact]
    public async Task A_buyer_can_read_their_purchase_but_another_user_cannot()
    {
        var purchase = PurchaseSeed.PurchaseOfExplorer;
        var owner = Factory.CreateClientFor(purchase.TouristId, "explorer");

        var response = await owner.GetAsync($"/api/payment/purchases/{purchase.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PurchaseDto>(JsonOptions);
        result!.CouponDiscount.Should().Be(purchase.CouponDiscount);
        var other = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var forbiddenResponse = await other.GetAsync($"/api/payment/purchases/{purchase.Id}");
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_reports_an_expired_coupon_without_applying_it()
    {
        var cart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var client = Factory.CreateClientFor(cart.TouristId, "explorer");

        var preview = await client.GetFromJsonAsync<CheckoutPreviewDto>(
            $"/api/payment/cart/checkout-preview?couponCode={CouponSeed.ExpiredCoupon.Code}",
            JsonOptions);

        preview!.CouponMessage.Should().NotBeNullOrWhiteSpace();
        preview.CouponDiscount.Should().Be(0);
        preview.Total.Should().Be(cart.Total);
    }

    [Fact]
    public async Task Anonymous_user_cannot_preview_checkout_benefits()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/cart/checkout-preview");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_preview_checkout_benefits()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/cart/checkout-preview");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
