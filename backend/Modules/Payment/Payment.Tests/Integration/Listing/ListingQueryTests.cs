using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.Listing;

public class ListingQueryTests : BaseIntegrationTest
{
    private static readonly Guid TourWithoutProduct = Guid.Parse("00000000-0000-0000-0004-000000000099");

    public ListingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Prices_are_returned_for_paid_and_free_tours_and_a_tour_without_a_product_is_left_out()
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var prices = await paymentApi.GetTourPricesAsync(
            [ProductSeed.PaidTour.EntityId, ProductSeed.FreeTour.EntityId, TourWithoutProduct]);

        prices.Should().Contain(ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Price.Value);
        prices.Should().Contain(ProductSeed.FreeTour.EntityId, 0);
        prices.Should().NotContainKey(TourWithoutProduct);
    }
}
