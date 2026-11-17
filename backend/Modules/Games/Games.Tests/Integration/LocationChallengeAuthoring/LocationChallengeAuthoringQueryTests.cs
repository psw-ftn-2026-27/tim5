using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.UploadedImages;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Challenge_is_shown_with_its_hidden_points_and_their_photo_addresses()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.DraftFortressHuntWithThreePoints;

        var challenge = await client.GetFromJsonAsync<LocationChallengeDto>($"/api/games/location-challenges/{seeded.Id}", JsonOptions);

        challenge!.Id.Should().Be(seeded.Id);
        challenge.HiddenPoints.Select(point => (point.Id, point.ImageId, point.ImageUrl))
            .Should().BeEquivalentTo(seeded.HiddenPoints.Select(point => (point.Id, point.ImageId, ImageAddress.For(point.ImageId))));
    }
}
