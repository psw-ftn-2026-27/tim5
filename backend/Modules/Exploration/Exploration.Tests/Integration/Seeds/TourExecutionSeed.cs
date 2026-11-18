using Exploration.Domain.Tours;

namespace Exploration.Tests.Integration.Seeds;

// The aggregate's name is qualified because, inside Exploration.Tests.Integration, the simple
// name TourExecution means the test folder's namespace.
internal static class TourExecutionSeed
{
    public static readonly Domain.TourExecutions.TourExecution ActiveOldTownOfTouristOnOldTown = new(
        TouristPositionSeed.TouristOnOldTownAtDanubePark.UserId,
        TourSeed.PublishedOldTown.Id,
        new Location(45.2551, 19.8451),
        new DateTime(2026, 11, 16, 9, 15, 0, DateTimeKind.Utc));

    public static readonly Domain.TourExecutions.TourExecution ActiveFortressWithClockTowerCompletedOfTouristOnFortress;
    public static readonly Domain.TourExecutions.TourExecution AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned;

    static TourExecutionSeed()
    {
        ActiveFortressWithClockTowerCompletedOfTouristOnFortress = new(
            TouristPositionSeed.TouristOnFortressAtGalleries.UserId,
            TourSeed.PublishedFortressWithSecrets.Id,
            new Location(45.2517, 19.8620),
            new DateTime(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc));
        ActiveFortressWithClockTowerCompletedOfTouristOnFortress.CompleteKeyPoint(
            TourSeed.PublishedFortressWithSecrets.KeyPoints[0].Id,
            new DateTime(2026, 11, 17, 9, 1, 0, DateTimeKind.Utc),
            false);

        AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned = new(
            TouristPositionSeed.FortressBuyerWhoAbandonedAtGalleries.UserId,
            TourSeed.PublishedFortressWithSecrets.Id,
            new Location(45.2517, 19.8620),
            new DateTime(2026, 11, 16, 13, 0, 0, DateTimeKind.Utc));
        AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned.CompleteKeyPoint(
            TourSeed.PublishedFortressWithSecrets.KeyPoints[0].Id,
            new DateTime(2026, 11, 16, 13, 1, 0, DateTimeKind.Utc),
            false);
        AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned.Abandon(new DateTime(2026, 11, 16, 13, 30, 0, DateTimeKind.Utc));
    }

    public static Domain.TourExecutions.TourExecution[] All =>
        [ActiveOldTownOfTouristOnOldTown, ActiveFortressWithClockTowerCompletedOfTouristOnFortress,
            AbandonedFortressWithClockTowerCompletedOfBuyerWhoAbandoned];
}
