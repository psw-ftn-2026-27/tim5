using System.Net;
using System.Net.Http.Json;
using Exploration.Application.PositionSimulator;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.PositionSimulator;

public class PositionSimulatorQueryTests : BaseIntegrationTest
{
    public PositionSimulatorQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_reads_their_marked_position_with_its_coordinates_and_time()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = TouristPositionSeed.SecondExplorerAtLibertySquare;

        var position = await client.GetFromJsonAsync<TouristPositionDto>("/api/exploration/positions/mine", JsonOptions);

        position.Should().Be(new TouristPositionDto(seeded.Location.Latitude, seeded.Location.Longitude, seeded.MarkedAt));
    }

    [Fact]
    public async Task Explorer_who_never_marked_a_position_learns_that_it_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync("/api/exploration/positions/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
