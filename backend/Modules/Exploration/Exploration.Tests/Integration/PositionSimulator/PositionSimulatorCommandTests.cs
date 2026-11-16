using System.Net;
using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.PositionSimulator;

public class PositionSimulatorCommandTests : BaseIntegrationTest
{
    public PositionSimulatorCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_marks_a_position_for_the_first_time_and_it_is_stored_with_its_coordinates_and_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2520, 19.8630));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TouristPositions.Count().Should().Be(positionCountBefore + 1);
        var stored = assertContext.TouristPositions.Single(position => position.UserId == WellKnownUsers.Explorer);
        stored.Location.Latitude.Should().Be(45.2520);
        stored.Location.Longitude.Should().Be(19.8630);
        stored.MarkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Explorer_marks_a_new_position_which_replaces_the_previous_one_and_remains_their_only_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = TouristPositionSeed.SecondExplorerAtLibertySquare;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count(position => position.UserId == WellKnownUsers.SecondExplorer);

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2448, 19.8562));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TouristPositions.Count(position => position.UserId == WellKnownUsers.SecondExplorer)
            .Should().Be(positionCountBefore);
        var stored = assertContext.TouristPositions.Single(position => position.UserId == WellKnownUsers.SecondExplorer);
        stored.Location.Latitude.Should().Be(45.2448);
        stored.Location.Longitude.Should().Be(19.8562);
        stored.MarkedAt.Should().BeAfter(seeded.MarkedAt);
    }

    [Fact]
    public async Task Explorer_marking_a_position_leaves_another_explorers_position_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TouristPositionSeed.SecondExplorerAtLibertySquare;

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2520, 19.8630));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TouristPositions.Single(position => position.Id == seeded.Id);
        stored.UserId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.Location.Latitude.Should().Be(seeded.Location.Latitude);
        stored.Location.Longitude.Should().Be(seeded.Location.Longitude);
        stored.MarkedAt.Should().Be(seeded.MarkedAt);
    }

    [Fact]
    public async Task Explorer_cannot_mark_a_position_outside_the_valid_coordinate_range()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2520, 199.8630));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TouristPositions.Count().Should().Be(positionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_mark_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2520, 19.8630));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_mark_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync("/api/exploration/positions/mine", new LocationDto(45.2520, 19.8630));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
