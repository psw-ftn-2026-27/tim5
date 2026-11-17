using Payment.Domain.TourPurchaseTokens;

namespace Exploration.Tests.Integration.Seeds;

// Payment's purchase tokens, so starting and previewing a tour get real answers through IPaymentApi.
// Named TourPurchaseSeed, not TourPurchaseTokenSeed, because Payment.Tests sees this namespace next to its own seeds.
internal static class TourPurchaseSeed
{
    public static readonly TourPurchaseToken OldTownBoughtByBuyerWithPosition = new(
        TouristPositionSeed.OldTownBuyerAtLibertySquare.UserId,
        TourSeed.PublishedOldTown.Id,
        new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken ArchivedSalasiBoughtByBuyerWithPosition = new(
        TouristPositionSeed.OldTownBuyerAtLibertySquare.UserId,
        TourSeed.ArchivedSalasi.Id,
        new DateTime(2026, 11, 15, 10, 5, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken OldTownBoughtByBuyerWithoutPosition = new(
        Guid.Parse("00000000-0000-0000-0008-000000000002"),
        TourSeed.PublishedOldTown.Id,
        new DateTime(2026, 11, 15, 11, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken OldTownBoughtByTouristOnOldTown = new(
        TouristPositionSeed.TouristOnOldTownAtDanubePark.UserId,
        TourSeed.PublishedOldTown.Id,
        new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc));

    public static TourPurchaseToken[] All =>
        [OldTownBoughtByBuyerWithPosition, ArchivedSalasiBoughtByBuyerWithPosition,
            OldTownBoughtByBuyerWithoutPosition, OldTownBoughtByTouristOnOldTown];
}
