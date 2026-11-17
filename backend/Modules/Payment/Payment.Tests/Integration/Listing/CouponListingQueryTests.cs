using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Listing;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Listing;

public class CouponListingQueryTests : BaseIntegrationTest
{
    public CouponListingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_explorer_sees_only_products_they_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var products = await client.GetFromJsonAsync<List<MyProductDto>>("/api/payment/products/mine", JsonOptions);

        products.Should().NotBeNull();
        products!.Select(product => product.ProductId).Should().BeEquivalentTo(new[]
        {
            ProductSeed.PaidTour.EntityId,
            ProductSeed.FreeTour.EntityId,
            ProductSeed.SecondPaidTour.EntityId
        });
        products.Should().NotContain(product => product.ProductId == ProductSeed.OtherOwnersTour.EntityId);
    }

    [Fact]
    public async Task An_anonymous_user_cannot_read_their_products()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/products/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_administrator_cannot_read_explorer_products()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/products/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
