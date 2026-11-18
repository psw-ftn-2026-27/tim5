using Exploration.Tests.Integration.Seeds;
using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

// The aggregate's name is qualified because, inside Games.Tests.Integration, the simple
// name LocationChallengeActivation means the test folder's namespace.
internal static class LocationChallengeActivationSeed
{
    public static readonly Domain.LocationChallengeActivations.LocationChallengeActivation FortressBuyerOnFortressGalleriesHunt =
        LocationChallengeSeed.PublishedFortressGalleriesHuntWithTwoPoints.Activate(
            TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.UserId,
            new Location(
                TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.Location.Latitude,
                TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.Location.Longitude),
            40);

    public static Domain.LocationChallengeActivations.LocationChallengeActivation[] All => [FortressBuyerOnFortressGalleriesHunt];
}
