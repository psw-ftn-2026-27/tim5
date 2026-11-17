using System.Net.Http.Json;
using System.Text.Json;
using Exploration.Application.TourExecution;
using Exploration.Application.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Xunit;

namespace Exploration.Tests.Integration.TourExecution;

public class TourExecutionQueryTests : BaseIntegrationTest
{
    public TourExecutionQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_execution_shows_its_tour_the_current_position_and_the_first_key_point()
    {
        var seeded = TourExecutionSeed.ActiveOldTownOfTouristOnOldTown;
        var position = TouristPositionSeed.TouristOnOldTownAtDanubePark;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var active = await client.GetFromJsonAsync<ActiveTourExecutionDto>("/api/exploration/executions/active", JsonOptions);

        var firstKeyPoint = TourSeed.PublishedOldTown.FirstKeyPoint()!;
        active!.TourId.Should().Be(TourSeed.PublishedOldTown.Id);
        active.TourName.Should().Be(TourSeed.PublishedOldTown.Name);
        active.CurrentPosition.Should().Be(new LocationDto(position.Location.Latitude, position.Location.Longitude));
        active.NextKeyPoint.Should().Be(new NextKeyPointDto(
            firstKeyPoint.Name,
            new LocationDto(firstKeyPoint.Location.Latitude, firstKeyPoint.Location.Longitude)));
    }

    [Fact]
    public async Task Active_execution_shows_the_secret_only_of_its_completed_key_points()
    {
        var seeded = TourExecutionSeed.ActiveFortressWithClockTowerCompletedOfTouristOnFortress;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var body = await client.GetStringAsync("/api/exploration/executions/active");

        var keyPoints = TourSeed.PublishedFortressWithSecrets.KeyPoints;
        var active = JsonSerializer.Deserialize<ActiveTourExecutionDto>(body, JsonOptions)!;
        active.CompletedKeyPoints.Should().Equal(new CompletedKeyPointDto(
            keyPoints[0].Id,
            keyPoints[0].Name,
            seeded.CompletedKeyPoints[0].ReachedAt,
            keyPoints[0].Secret));
        active.NextKeyPoint.Name.Should().Be(keyPoints[1].Name);
        body.Should().NotContain(keyPoints[1].Secret!);
        body.Should().NotContain(keyPoints[2].Secret!);
    }
}
