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

    public static readonly Domain.LocationChallengeActivations.LocationChallengeActivation FortressBuyerOnGalleriesPassageHuntWithNothingDiscovered =
        LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints.Activate(
            TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.UserId,
            new Location(
                TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.Location.Latitude,
                TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.Location.Longitude),
            30);

    public static readonly Domain.LocationChallengeActivations.LocationChallengeActivation TouristOnFortressOnGalleriesPassageHuntWithTheGalleriesPointDiscovered;

    static LocationChallengeActivationSeed()
    {
        var galleriesPosition = new Location(
            TouristPositionSeed.TouristOnFortressAtGalleries.Location.Latitude,
            TouristPositionSeed.TouristOnFortressAtGalleries.Location.Longitude);
        TouristOnFortressOnGalleriesPassageHuntWithTheGalleriesPointDiscovered =
            LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints.Activate(
                TouristPositionSeed.TouristOnFortressAtGalleries.UserId,
                galleriesPosition,
                30);
        Domain.LocationChallengeActivations.LocationChecker.Check(
            LocationChallengeSeed.PublishedGalleriesPassageHuntWithTwoPoints,
            TouristOnFortressOnGalleriesPassageHuntWithTheGalleriesPointDiscovered,
            galleriesPosition,
            new DateTime(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc));
    }

    public static Domain.LocationChallengeActivations.LocationChallengeActivation[] All =>
    [
        FortressBuyerOnFortressGalleriesHunt,
        FortressBuyerOnGalleriesPassageHuntWithNothingDiscovered,
        TouristOnFortressOnGalleriesPassageHuntWithTheGalleriesPointDiscovered
    ];
}
