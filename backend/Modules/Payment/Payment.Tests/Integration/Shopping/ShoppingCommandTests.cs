using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Shopping;
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
}
