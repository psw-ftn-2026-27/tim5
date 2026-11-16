using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Purchasing;
using Payment.Contracts;
using Payment.Domain.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
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

    [Fact]
    public async Task Tourist_sees_exactly_their_own_purchased_tours_with_the_name_from_the_purchase_and_the_time_from_the_token_from_newest_to_oldest()
    {
        var wallet = WalletSeed.WalletWithTwoPurchases;
        var client = Factory.CreateClientFor(wallet.TouristId, "explorer");
        var expected = TourPurchaseTokenSeed.All
            .Where(token => token.TouristId == wallet.TouristId)
            .OrderByDescending(token => token.PurchasedAt)
            .Select(token => new PurchasedTourDto(
                token.EntityId,
                wallet.Transactions
                    .Single(transaction => transaction.Type == TransactionType.Purchase && transaction.EntityId == token.EntityId)
                    .Name!,
                token.PurchasedAt))
            .ToList();

        var purchases = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/payment/purchases", JsonOptions);

        purchases.Should().Equal(expected);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_purchased_tours()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/purchases");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_purchased_tours_as_a_tourist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/purchases");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
