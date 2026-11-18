using System.Net.Http.Json;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Games.Application.LocationChallengeBrowsing;
using Games.Domain.SocialChallenges;
using Games.Tests.Integration.Seeds;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeBrowsing;

public class LocationChallengeBrowsingQueryTests : BaseIntegrationTest
{
    public LocationChallengeBrowsingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Nearby_lists_published_challenges_but_not_drafts_and_marks_the_ones_the_user_already_activated()
    {
        var userId = TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.UserId;
        var client = Factory.CreateClientFor(userId, "explorer");

        var nearby = await client.GetFromJsonAsync<NearbyLocationChallengesDto>("/api/games/location-challenges/nearby", JsonOptions);

        var expected = LocationChallengeSeed.All
            .Where(challenge => challenge.Status == ChallengeStatus.Published)
            .Select(challenge => (
                challenge.Id,
                IsActivated: LocationChallengeActivationSeed.All
                    .Any(activation => activation.ChallengeId == challenge.Id && activation.UserId == userId)));
        nearby!.Challenges.Select(challenge => (challenge.Id, challenge.IsActivated))
            .Should().BeEquivalentTo(expected);
    }
}
