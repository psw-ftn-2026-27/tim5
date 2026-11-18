using System.Net;
using Exploration.Domain.TourExecutions;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecution;

public class TourExecutionCommandTests : BaseIntegrationTest
{
    public TourExecutionCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_who_starts_a_purchased_tour_gets_a_stored_active_execution_from_their_position()
    {
        var buyer = TouristPositionSeed.OldTownBuyerAtLibertySquare;
        var client = Factory.CreateClientFor(buyer.UserId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count(execution => execution.TouristId == buyer.UserId);

        var response = await client.PostAsync($"/api/exploration/executions/tours/{TourSeed.PublishedOldTown.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count(execution => execution.TouristId == buyer.UserId).Should().Be(executionCountBefore + 1);
        var stored = assertContext.TourExecutions.Single(execution => execution.TouristId == buyer.UserId);
        stored.TourId.Should().Be(TourSeed.PublishedOldTown.Id);
        stored.Status.Should().Be(TourExecutionStatus.Active);
        stored.StartPosition.Should().Be(buyer.Location);
        stored.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Starting_a_tour_without_a_marked_position_is_rejected_and_no_execution_is_created()
    {
        var buyerWithoutPosition = TourPurchaseSeed.OldTownBoughtByBuyerWithoutPosition.TouristId;
        var client = Factory.CreateClientFor(buyerWithoutPosition, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsync($"/api/exploration/executions/tours/{TourSeed.PublishedOldTown.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Starting_a_tour_the_explorer_has_not_purchased_is_rejected_and_no_execution_is_created()
    {
        var buyer = TouristPositionSeed.OldTownBuyerAtLibertySquare;
        var client = Factory.CreateClientFor(buyer.UserId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsync($"/api/exploration/executions/tours/{TourSeed.PublishedVineyards.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/executions/tours/{TourSeed.PublishedOldTown.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/executions/tours/{TourSeed.PublishedOldTown.Id}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Abandoning_the_active_tour_stores_the_execution_as_abandoned_with_its_abandoning_time()
    {
        var seeded = TourExecutionSeed.ActiveOldTownOfTouristOnOldTown;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var response = await client.PostAsync("/api/exploration/executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == seeded.Id);
        stored.Status.Should().Be(TourExecutionStatus.Abandoned);
        stored.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Abandoning_without_an_active_tour_reports_that_it_does_not_exist_and_abandons_nothing()
    {
        var client = Factory.CreateClientFor(TouristPositionSeed.OldTownBuyerAtLibertySquare.UserId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var abandonedCountBefore = arrangeContext.TourExecutions.Count(execution => execution.Status == TourExecutionStatus.Abandoned);

        var response = await client.PostAsync("/api/exploration/executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count(execution => execution.Status == TourExecutionStatus.Abandoned).Should().Be(abandonedCountBefore);
    }

    [Fact]
    public async Task Position_check_near_the_next_key_point_stores_it_as_completed_in_the_execution()
    {
        var seeded = TourExecutionSeed.ActiveFortressWithClockTowerCompletedOfTouristOnFortress;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var completedCountBefore = arrangeContext.TourExecutions.Single(execution => execution.Id == seeded.Id).CompletedKeyPoints.Count;

        var response = await client.PostAsync("/api/exploration/executions/active/position-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == seeded.Id);
        stored.CompletedKeyPoints.Should().HaveCount(completedCountBefore + 1);
        var galleries = TourSeed.PublishedFortressWithSecrets.KeyPoints[1];
        var completed = stored.CompletedKeyPoints.Single(completedKeyPoint => completedKeyPoint.KeyPointId == galleries.Id);
        completed.ReachedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        stored.LastActivity.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Position_check_without_an_active_tour_reports_that_it_does_not_exist_and_changes_no_earlier_execution()
    {
        var abandoned = TourExecutionSeed.AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned;
        var client = Factory.CreateClientFor(abandoned.TouristId, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var before = arrangeContext.TourExecutions.Single(execution => execution.Id == abandoned.Id);
        var completedCountBefore = before.CompletedKeyPoints.Count;
        var lastActivityBefore = before.LastActivity;

        var response = await client.PostAsync("/api/exploration/executions/active/position-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == abandoned.Id);
        stored.CompletedKeyPoints.Should().HaveCount(completedCountBefore);
        stored.LastActivity.Should().Be(lastActivityBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_position_on_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/exploration/executions/active/position-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_position_on_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/exploration/executions/active/position-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/exploration/executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/exploration/executions/active/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
