using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using Games.Application.LocationChallengeActivation;
using Games.Application.Quizzes;
using Games.Application.UploadedImages;
using Games.Tests.Integration.Seeds;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeActivation;

public class LocationChallengeActivationQueryTests : BaseIntegrationTest
{
    public LocationChallengeActivationQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_challenge_shows_the_circle_and_photo_addresses_but_not_the_hidden_point_locations()
    {
        var activation = LocationChallengeActivationSeed.FortressBuyerOnFortressGalleriesHunt;
        var challenge = LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints;
        var client = Factory.CreateClientFor(activation.UserId, "explorer");

        var response = await client.GetAsync($"/api/games/location-challenges/{challenge.Id}/activation");

        var body = await response.Content.ReadAsStringAsync();
        var active = JsonSerializer.Deserialize<ActiveLocationChallengeDto>(body, JsonOptions);
        active!.ChallengeId.Should().Be(challenge.Id);
        active.Center.Latitude.Should().Be(challenge.StartLocation.Latitude);
        active.Center.Longitude.Should().Be(challenge.StartLocation.Longitude);
        active.RadiusInMeters.Should().BeApproximately(activation.Radius, 0.001);
        active.Photos.Select(photo => photo.PhotoUrl)
            .Should().BeEquivalentTo(challenge.HiddenPoints.Select(point => ImageAddress.For(point.ImageId)));
        foreach (var point in challenge.HiddenPoints)
        {
            body.Should().NotContain(point.Location.Latitude.ToString(CultureInfo.InvariantCulture));
            body.Should().NotContain(point.Location.Longitude.ToString(CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public async Task Active_challenge_shows_the_location_of_a_discovered_point_but_not_of_an_undiscovered_one()
    {
        var activation = LocationChallengeActivationSeed.TouristOnFortressOnGalleriesPassageHuntWithTheGalleriesPointDiscovered;
        var challenge = LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints;
        var discoveredPoint = challenge.HiddenPoints.Single(point => activation.DiscoveredPointIds.Contains(point.Id));
        var undiscoveredPoint = challenge.HiddenPoints.Single(point => !activation.DiscoveredPointIds.Contains(point.Id));
        var client = Factory.CreateClientFor(activation.UserId, "explorer");

        var response = await client.GetAsync($"/api/games/location-challenges/{challenge.Id}/activation");

        var body = await response.Content.ReadAsStringAsync();
        var active = JsonSerializer.Deserialize<ActiveLocationChallengeDto>(body, JsonOptions);
        active!.DiscoveredPointCount.Should().Be(activation.DiscoveredPointIds.Count);
        var discoveredPhoto = active.Photos.Single(photo => photo.PhotoUrl == ImageAddress.For(discoveredPoint.ImageId));
        discoveredPhoto.IsDiscovered.Should().BeTrue();
        discoveredPhoto.Location.Should().Be(new LocationDto(discoveredPoint.Location.Latitude, discoveredPoint.Location.Longitude));
        var undiscoveredPhoto = active.Photos.Single(photo => photo.PhotoUrl == ImageAddress.For(undiscoveredPoint.ImageId));
        undiscoveredPhoto.IsDiscovered.Should().BeFalse();
        undiscoveredPhoto.Location.Should().BeNull();
        body.Should().NotContain(undiscoveredPoint.Location.Latitude.ToString(CultureInfo.InvariantCulture));
        body.Should().NotContain(undiscoveredPoint.Location.Longitude.ToString(CultureInfo.InvariantCulture));
    }
}
