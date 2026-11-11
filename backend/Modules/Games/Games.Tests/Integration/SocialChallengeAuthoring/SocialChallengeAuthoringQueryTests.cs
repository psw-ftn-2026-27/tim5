using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringQueryTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Overview_lists_all_stored_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>("/api/games/challenges", JsonOptions);

        challenges!.Select(challenge => challenge.Id)
            .Should().BeEquivalentTo(SocialChallengeSeed.All.Select(challenge => challenge.Id));
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public async Task Overview_filtered_by_status_lists_only_challenges_in_that_status(ChallengeStatus status)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<SocialChallengeDto>>($"/api/games/challenges?status={status}", JsonOptions);

        challenges!.Select(challenge => challenge.Id)
            .Should().BeEquivalentTo(SocialChallengeSeed.All.Where(challenge => challenge.Status == status).Select(challenge => challenge.Id));
    }
}
