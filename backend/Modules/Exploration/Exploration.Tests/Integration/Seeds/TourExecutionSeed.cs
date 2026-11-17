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

    public static Domain.TourExecutions.TourExecution[] All => [ActiveOldTownOfTouristOnOldTown];
}
