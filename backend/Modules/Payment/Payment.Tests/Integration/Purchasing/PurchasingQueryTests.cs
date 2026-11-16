using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.Purchasing;

public class PurchasingQueryTests : BaseIntegrationTest
{
    public PurchasingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Purchase_is_confirmed_for_a_tourist_and_tour_with_a_purchase_token()
    {
        var token = TourPurchaseTokenSeed.PaidTourPurchasedByTouristWithoutCart;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(token.TouristId, token.EntityId);

        hasPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Purchase_is_not_confirmed_for_a_tour_the_tourist_has_no_purchase_token_for()
    {
        var token = TourPurchaseTokenSeed.PaidTourPurchasedByTouristWithoutCart;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var hasPurchased = await paymentApi.HasPurchasedAsync(token.TouristId, ProductSeed.SecondPaidTour.EntityId);

        hasPurchased.Should().BeFalse();
    }
}
