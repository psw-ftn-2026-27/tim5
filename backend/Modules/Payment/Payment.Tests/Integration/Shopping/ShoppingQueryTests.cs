using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Shopping;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Shopping;

public class ShoppingQueryTests : BaseIntegrationTest
{
    private static readonly Guid TourWithoutProduct = Guid.Parse("00000000-0000-0000-0004-000000000099");

    public ShoppingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_sees_only_the_tours_in_their_own_cart_and_their_total_while_another_tourist_has_a_cart()
    {
        var seeded = ShoppingCartSeed.CartWithTwoTours;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");
        var expected = seeded.Items
            .Select(item => new OrderItemDto(item.EntityId, item.Name, item.Price.Value))
            .ToList();

        var cart = await client.GetFromJsonAsync<ShoppingCartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Should().BeEquivalentTo(expected);
        cart.Total.Should().Be(seeded.Items.Sum(item => item.Price.Value));
    }

    [Fact]
    public async Task Tourist_without_a_cart_sees_no_tours_and_a_total_of_zero()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var cart = await client.GetFromJsonAsync<ShoppingCartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
    }

    [Fact]
    public async Task Product_of_a_tour_without_a_product_is_rejected_as_not_existing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/payment/products/{TourWithoutProduct}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
