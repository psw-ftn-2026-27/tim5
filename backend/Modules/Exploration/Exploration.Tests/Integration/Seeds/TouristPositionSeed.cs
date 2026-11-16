using Exploration.Domain.TouristPositions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TouristPositionSeed
{
    public static readonly TouristPosition SecondExplorerAtLibertySquare = new(WellKnownUsers.SecondExplorer, new Location(45.2551, 19.8451), new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));

    public static TouristPosition[] All => [SecondExplorerAtLibertySquare];
}
