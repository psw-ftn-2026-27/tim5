using System.Net;
using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeLifecycle;

public class LocationChallengeLifecycleCommandTests : BaseIntegrationTest
{
    public LocationChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_challenge_with_hidden_points_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Draft_challenge_without_hidden_points_cannot_be_published_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id;

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Publishing_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/games/location-challenges/{missingId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_challenge_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id;

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_published_challenge_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id;

        var response = await client.PostAsync($"/api/games/location-challenges/{challengeId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/location-challenges/{LocationChallengeSeed.PublishedDanubeQuayHuntWithOnePoint.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_challenge_and_neither_it_nor_its_hidden_points_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = LocationChallengeSeed.DraftFortressHuntWithThreePoints;
        var seededPointIds = seeded.HiddenPoints.Select(point => point.Id).ToList();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();
        var pointCountBefore = arrangeContext.Set<HiddenPoint>().Count();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{seeded.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == seeded.Id);
        assertContext.Set<HiddenPoint>().Count().Should().Be(pointCountBefore - seededPointIds.Count);
        assertContext.Set<HiddenPoint>().Should().NotContain(point => seededPointIds.Contains(point.Id));
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_location_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
