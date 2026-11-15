using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.ChallengeOverview;
using Games.Domain.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.ChallengeOverview;

public class ChallengeOverviewQueryTests : BaseIntegrationTest
{
    public ChallengeOverviewQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Overview_lists_all_quizzes_and_social_challenges_with_their_kind()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<ChallengeOverviewItemDto>>("/api/games/overview", JsonOptions);

        var expected = QuizSeed.All.Select(quiz => (quiz.Id, ChallengeKind.Quiz))
            .Concat(SocialChallengeSeed.All.Select(challenge => (challenge.Id, ChallengeKind.SocialChallenge)));
        challenges!.Select(challenge => (challenge.Id, challenge.Kind))
            .Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public async Task Overview_filtered_by_status_lists_only_challenges_of_both_kinds_in_that_status(ChallengeStatus status)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<ChallengeOverviewItemDto>>($"/api/games/overview?status={status}", JsonOptions);

        var expected = QuizSeed.All.Where(quiz => quiz.Status == status).Select(quiz => quiz.Id)
            .Concat(SocialChallengeSeed.All.Where(challenge => challenge.Status == status).Select(challenge => challenge.Id));
        challenges!.Select(challenge => challenge.Id)
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Explorer_cannot_see_the_overview()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync("/api/games/overview");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
