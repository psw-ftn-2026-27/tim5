using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Shopping;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Shopping;

public class ShoppingCommandTests : BaseIntegrationTest
{
    private static readonly Guid TouristWithoutCart = Guid.Parse("00000000-0000-0000-0005-000000000099");
    private static readonly Guid TourWithoutProduct = Guid.Parse("00000000-0000-0000-0004-000000000099");

    public ShoppingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_with_a_cart_adds_a_tour_and_the_cart_holds_it_with_the_product_name_and_price()
    {
        var seeded = ShoppingCartSeed.SecondExplorersCartWithPaidTour;
        var product = ProductSeed.SecondPaidTour;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(product.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.ShoppingCarts.Single(cart => cart.Id == seeded.Id);
        stored.Items.Should().HaveCount(seeded.Items.Count + 1);
        stored.Items.Should().ContainSingle(item =>
            item.EntityId == product.EntityId
            && item.Name == product.Name
            && item.Price.Value == product.Price.Value);
    }

    [Fact]
    public async Task Tourist_without_a_cart_adds_a_tour_and_gets_a_cart_with_that_one_tour()
    {
        var product = ProductSeed.PaidTour;
        var client = Factory.CreateClientFor(TouristWithoutCart, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(product.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore + 1);
        var stored = assertContext.ShoppingCarts.Single(cart => cart.TouristId == TouristWithoutCart);
        stored.Items.Should().ContainSingle(item => item.EntityId == product.EntityId);
    }

    [Fact]
    public async Task Adding_a_tour_without_a_product_is_rejected_and_leaves_the_carts_unchanged()
    {
        var seeded = ShoppingCartSeed.SecondExplorersCartWithPaidTour;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(TourWithoutProduct));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        var stored = assertContext.ShoppingCarts.Single(cart => cart.Id == seeded.Id);
        stored.Items.Should().HaveSameCount(seeded.Items);
        stored.Items.Should().NotContain(item => item.EntityId == TourWithoutProduct);
    }

    [Fact]
    public async Task Adding_a_tour_that_is_already_in_the_cart_is_rejected_and_leaves_the_cart_unchanged()
    {
        var seeded = ShoppingCartSeed.SecondExplorersCartWithPaidTour;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.PaidTour.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.ShoppingCarts.Single(cart => cart.Id == seeded.Id);
        stored.Items.Should().HaveSameCount(seeded.Items);
    }

    [Fact]
    public async Task Tourist_removes_a_tour_and_it_is_no_longer_in_their_cart()
    {
        var seeded = ShoppingCartSeed.SecondExplorersCartWithPaidTour;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var response = await client.DeleteAsync($"/api/payment/cart/items/{ProductSeed.PaidTour.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.ShoppingCarts.Single(cart => cart.Id == seeded.Id);
        stored.Items.Should().NotContain(item => item.EntityId == ProductSeed.PaidTour.EntityId);
    }

    [Fact]
    public async Task Removing_for_a_tourist_without_a_cart_is_rejected_and_creates_no_cart()
    {
        var client = Factory.CreateClientFor(TouristWithoutCart, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.DeleteAsync($"/api/payment/cart/items/{ProductSeed.PaidTour.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        assertContext.ShoppingCarts.Should().NotContain(cart => cart.TouristId == TouristWithoutCart);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_tour_to_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.PaidTour.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_tour_to_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.PaidTour.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_tour_from_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/payment/cart/items/{ProductSeed.PaidTour.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_tour_from_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/payment/cart/items/{ProductSeed.PaidTour.EntityId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Adding_a_tour_the_tourist_has_already_purchased_is_rejected_and_creates_no_cart()
    {
        var token = TourPurchaseTokenSeed.PaidTourPurchasedByTouristWithoutCart;
        var client = Factory.CreateClientFor(token.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var cartCountBefore = arrangeContext.ShoppingCarts.Count();

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(token.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Count().Should().Be(cartCountBefore);
        assertContext.ShoppingCarts.Should().NotContain(cart => cart.TouristId == token.TouristId);
    }

    [Fact]
    public async Task Tourist_pays_the_cart_which_empties_it_lowers_the_balance_by_the_total_and_records_a_purchase_and_a_token_for_each_tour()
    {
        var seededCart = ShoppingCartSeed.BuyersCartWithEnoughEC;
        var seededWallet = WalletSeed.BuyersWalletWithEnoughEC;
        var client = Factory.CreateClientFor(seededCart.TouristId, "explorer");

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.Id == seededCart.Id).Items.Should().BeEmpty();
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == seededWallet.Id);
        storedWallet.Balance.Should().Be(seededWallet.Balance - seededCart.Total);
        storedWallet.Transactions
            .Where(transaction => transaction.Type == TransactionType.Purchase)
            .Select(transaction => new { transaction.EntityId, transaction.Name, Amount = transaction.Amount.Value })
            .Should().BeEquivalentTo(seededCart.Items
                .Select(item => new { EntityId = (Guid?)item.EntityId, Name = (string?)item.Name, Amount = item.Price.Value }));
        assertContext.TourPurchaseTokens
            .Where(token => token.TouristId == seededCart.TouristId)
            .Select(token => token.EntityId)
            .Should().BeEquivalentTo(seededCart.Items.Select(item => item.EntityId));
    }

    [Fact]
    public async Task Paying_with_too_few_EC_is_rejected_and_leaves_the_cart_the_wallet_and_the_tokens_unchanged()
    {
        var seededCart = ShoppingCartSeed.BuyersCartShortOfEC;
        var seededWallet = WalletSeed.BuyersWalletShortOfEC;
        var client = Factory.CreateClientFor(seededCart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var tokenCountBefore = arrangeContext.TourPurchaseTokens.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.Id == seededCart.Id).Items.Should().HaveSameCount(seededCart.Items);
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == seededWallet.Id);
        storedWallet.Balance.Should().Be(seededWallet.Balance);
        storedWallet.Transactions.Should().HaveSameCount(seededWallet.Transactions);
        assertContext.TourPurchaseTokens.Count().Should().Be(tokenCountBefore);
    }

    [Fact]
    public async Task Paying_a_cart_with_a_tour_archived_in_exploration_is_rejected_and_leaves_the_cart_the_wallet_and_the_tokens_unchanged()
    {
        var seededCart = ShoppingCartSeed.BuyersCartWithArchivedTour;
        var seededWallet = WalletSeed.BuyersWalletWithArchivedTourInCart;
        var client = Factory.CreateClientFor(seededCart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var transactionCountBefore = arrangeContext.Wallets.Single(wallet => wallet.Id == seededWallet.Id).Transactions.Count;
        var tokenCountBefore = arrangeContext.TourPurchaseTokens.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts.Single(cart => cart.Id == seededCart.Id).Items
            .Select(item => item.EntityId)
            .Should().BeEquivalentTo(seededCart.Items.Select(item => item.EntityId));
        var storedWallet = assertContext.Wallets.Single(wallet => wallet.Id == seededWallet.Id);
        storedWallet.Balance.Should().Be(seededWallet.Balance);
        storedWallet.Transactions.Should().HaveCount(transactionCountBefore);
        assertContext.TourPurchaseTokens.Count().Should().Be(tokenCountBefore);
    }

    [Fact]
    public async Task Paying_for_a_tourist_without_a_wallet_is_rejected_and_creates_no_wallet()
    {
        var seededCart = ShoppingCartSeed.CartWithTwoTours;
        var client = Factory.CreateClientFor(seededCart.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        assertContext.Wallets.Should().NotContain(wallet => wallet.TouristId == seededCart.TouristId);
        assertContext.ShoppingCarts.Single(cart => cart.Id == seededCart.Id).Items.Should().HaveSameCount(seededCart.Items);
    }

    [Fact]
    public async Task Anonymous_user_cannot_pay_a_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_pay_a_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/payment/cart/checkout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
