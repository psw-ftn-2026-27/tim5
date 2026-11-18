using System.Net;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeActivation;

public class LocationChallengeActivationCommandTests : BaseIntegrationTest
{
    public LocationChallengeActivationCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_activates_a_nearby_published_challenge_and_the_activation_is_stored_with_its_radius()
    {
        var userId = TouristPositionSeed.TouristOnFortressAtGalleries.UserId;
        var client = Factory.CreateClientFor(userId, "explorer");
        var challenge = LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints;
        var farthestPointDistance = challenge.HiddenPoints.Max(point => challenge.StartLocation.DistanceTo(point.Location));
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var activationCountBefore = arrangeContext.LocationChallengeActivations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeActivations.Count().Should().Be(activationCountBefore + 1);
        var stored = assertContext.LocationChallengeActivations
            .Single(activation => activation.ChallengeId == challenge.Id && activation.UserId == userId);
        stored.Radius.Should().BeInRange(farthestPointDistance, farthestPointDistance + 100);
    }

    [Fact]
    public async Task Activation_without_a_saved_position_is_rejected_and_no_activation_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challengeId = LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var activationCountBefore = arrangeContext.LocationChallengeActivations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeActivations.Count().Should().Be(activationCountBefore);
        assertContext.LocationChallengeActivations.Should().NotContain(activation => activation.UserId == WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Activation_from_more_than_50_meters_away_is_rejected_and_no_activation_is_stored()
    {
        var userId = TouristPositionSeed.SecondExplorerAtLibertySquare.UserId;
        var client = Factory.CreateClientFor(userId, "explorer");
        var challengeId = LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var activationCountBefore = arrangeContext.LocationChallengeActivations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeActivations.Count().Should().Be(activationCountBefore);
        assertContext.LocationChallengeActivations.Should().NotContain(activation => activation.UserId == userId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints.Id}/activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_at_a_hidden_location_checks_and_the_point_is_stored_as_discovered()
    {
        var activation = LocationChallengeActivationSeed.FortressBuyerOnGalleriesPassageHuntWithNothingDiscovered;
        var challenge = LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints;
        var client = Factory.CreateClientFor(activation.UserId, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{challenge.Id}/activation/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeActivations.Single(row => row.Id == activation.Id);
        stored.DiscoveredPointIds.Should().Equal(challenge.HiddenPoints[0].Id);
    }

    [Fact]
    public async Task Explorer_who_has_not_activated_the_challenge_cannot_check_and_nothing_is_stored()
    {
        var userId = TouristPositionSeed.SecondExplorerAtLibertySquare.UserId;
        var client = Factory.CreateClientFor(userId, "explorer");
        var challengeId = LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var activationCountBefore = arrangeContext.LocationChallengeActivations.Count();

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/activation/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallengeActivations.Count().Should().Be(activationCountBefore);
        assertContext.LocationChallengeActivations.Should().NotContain(activation => activation.UserId == userId);
    }

    [Fact]
    public async Task Check_far_from_all_hidden_points_is_rejected_and_the_activation_stays_the_same()
    {
        var activation = LocationChallengeActivationSeed.FortressBuyerOnFortressGalleriesHunt;
        var client = Factory.CreateClientFor(activation.UserId, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{activation.ChallengeId}/activation/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallengeActivations.Single(row => row.Id == activation.Id);
        stored.DiscoveredPointIds.Should().BeEmpty();
        stored.Completion.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_location()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints.Id}/activation/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints.Id}/activation/location-check", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
