using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge DraftQuayHuntWithoutPoints = new("Skrivena mesta na keju", 40, new Location(45.2480, 19.8560));
    public static readonly LocationChallenge DraftFortressHuntWithThreePoints;

    static LocationChallengeSeed()
    {
        DraftFortressHuntWithThreePoints = new("Skrivena mesta tvrđave", 70, new Location(45.2517, 19.8628));
        DraftFortressHuntWithThreePoints.AddHiddenPoint(UploadedImageSeed.AdministratorsLinkedFortressGatePhoto.Id, new Location(45.2522, 19.8615));
        DraftFortressHuntWithThreePoints.AddHiddenPoint(UploadedImageSeed.AdministratorsLinkedUndergroundTunnelPhoto.Id, new Location(45.2509, 19.8641));
        DraftFortressHuntWithThreePoints.AddHiddenPoint(UploadedImageSeed.AdministratorsLinkedArtistCellarPhoto.Id, new Location(45.2530, 19.8633));
    }

    public static LocationChallenge[] All => [DraftQuayHuntWithoutPoints, DraftFortressHuntWithThreePoints];
}
