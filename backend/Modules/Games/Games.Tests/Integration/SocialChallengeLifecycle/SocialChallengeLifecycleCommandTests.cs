using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeLifecycle;

public class SocialChallengeLifecycleCommandTests : BaseIntegrationTest
{
    public SocialChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_challenge_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSunsetGathering.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Publishing_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/games/challenges/{missingId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Published_challenge_cannot_be_published_again_and_stays_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedMorningRun.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedMorningRun.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_challenge_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedMorningRun.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedMorningRun.Id);
        stored.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Fact]
    public async Task Retiring_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/games/challenges/{missingId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Draft_challenge_cannot_be_retired_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSunsetGathering.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedMorningRun.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedMorningRun.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_challenge_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredMarketMeetup.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredMarketMeetup.Id);
        stored.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Fact]
    public async Task Archiving_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/games/challenges/{missingId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Draft_challenge_cannot_be_archived_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSunsetGathering.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredMarketMeetup.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/challenges/{SocialChallengeSeed.RetiredMarketMeetup.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_challenge_and_it_is_no_longer_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.ArchivedRiverPicnic.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == SocialChallengeSeed.ArchivedRiverPicnic.Id);
    }

    [Fact]
    public async Task Deleting_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/challenges/{missingId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Published_challenge_cannot_be_deleted_and_stays_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.PublishedMorningRun.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedMorningRun.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.ArchivedRiverPicnic.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/challenges/{SocialChallengeSeed.ArchivedRiverPicnic.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
