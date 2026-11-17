using Exploration.Domain.TouristPositions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TouristPositionSeed
{
    public static readonly TouristPosition SecondExplorerAtLibertySquare = new(WellKnownUsers.SecondExplorer, new Location(45.2551, 19.8451), new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));

    public static readonly TouristPosition OldTownBuyerAtLibertySquare = new(Guid.Parse("00000000-0000-0000-0008-000000000001"), new Location(45.2551, 19.8451), new DateTime(2026, 11, 16, 9, 0, 0, DateTimeKind.Utc));
    public static readonly TouristPosition TouristOnOldTownAtDanubePark = new(Guid.Parse("00000000-0000-0000-0008-000000000003"), new Location(45.2550, 19.8508), new DateTime(2026, 11, 16, 9, 30, 0, DateTimeKind.Utc));

    public static readonly TouristPosition TouristOnFortressAtGalleries = new(Guid.Parse("00000000-0000-0000-0008-000000000004"), new Location(45.2530, 19.8636), new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));
    public static readonly TouristPosition FortressBuyerWhoAbandonedAtGalleries = new(Guid.Parse("00000000-0000-0000-0008-000000000005"), new Location(45.2530, 19.8636), new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static TouristPosition[] All =>
        [SecondExplorerAtLibertySquare, OldTownBuyerAtLibertySquare, TouristOnOldTownAtDanubePark,
            TouristOnFortressAtGalleries, FortressBuyerWhoAbandonedAtGalleries];
}
