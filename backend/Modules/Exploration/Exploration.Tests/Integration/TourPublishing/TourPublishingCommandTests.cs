using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPublishing;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPublishing;

public class TourPublishingCommandTests : BaseIntegrationTest
{
    public TourPublishingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_publishes_a_tour_with_a_paid_price_and_payment_lists_it_at_that_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PriceDto(false, 500);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
        var listedPrices = await GetListedPricesAsync(TourSeed.PublishableRiverside.Id);
        listedPrices.Should().Contain(TourSeed.PublishableRiverside.Id, 500);
    }

    [Fact]
    public async Task Free_tour_is_listed_in_payment_at_a_zero_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PriceDto(true, 0);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var listedPrices = await GetListedPricesAsync(TourSeed.PublishableRiverside.Id);
        listedPrices.Should().Contain(TourSeed.PublishableRiverside.Id, 0);
    }

    [Fact]
    public async Task Tour_that_fails_the_publishing_rules_stays_a_draft_and_is_not_listed_in_payment()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PriceDto(false, 500);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        var listedPrices = await GetListedPricesAsync(TourSeed.FortressWalk.Id);
        listedPrices.Should().NotContainKey(TourSeed.FortressWalk.Id);
    }

    [Fact]
    public async Task Paid_tour_with_a_zero_amount_is_not_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new PriceDto(false, 0);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var request = new PriceDto(false, 500);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new PriceDto(false, 500);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_publish_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new PriceDto(false, 500);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<IReadOnlyDictionary<Guid, int>> GetListedPricesAsync(Guid tourId)
    {
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        return await paymentApi.GetTourPricesAsync([tourId]);
    }
}
