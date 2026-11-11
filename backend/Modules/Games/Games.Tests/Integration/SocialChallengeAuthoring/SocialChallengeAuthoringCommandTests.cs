using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static SocialChallengeInputDto CreateValidInput() =>
        new("Jutarnje trčanje", "Svaka čast, ranoranioci!", 4, 300, 40, new TimeWindowDto(new TimeOnly(6, 0), new TimeOnly(8, 0)));

    [Fact]
    public async Task Administrator_creates_a_challenge_that_is_stored_as_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<SocialChallengeDto>(JsonOptions);
        created!.Status.Should().Be(ChallengeStatus.Draft);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == created.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.Window.Should().Be(new TimeWindow(request.Window.Start, request.Window.End));
    }

    [Fact]
    public async Task Challenge_with_invalid_data_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput() with { Name = "   " };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/challenges", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/challenges", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_challenge_and_the_new_values_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput();

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSunsetGathering.Id);
        stored.Name.Should().Be(request.Name);
        stored.ExitMessage.Should().Be(request.ExitMessage);
        stored.RequiredPeople.Should().Be(request.RequiredPeople);
        stored.RangeInMeters.Should().Be(request.RangeInMeters);
        stored.Xp.Should().Be(request.Xp);
        stored.Window.Should().Be(new TimeWindow(request.Window.Start, request.Window.End));
    }

    [Fact]
    public async Task Changing_a_challenge_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{missingId}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.SocialChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Changing_a_challenge_with_invalid_data_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput() with { Xp = 0 };

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSunsetGathering.Id);
        stored.Name.Should().Be(SocialChallengeSeed.DraftSunsetGathering.Name);
        stored.Xp.Should().Be(SocialChallengeSeed.DraftSunsetGathering.Xp);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/challenges/{SocialChallengeSeed.DraftSunsetGathering.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
