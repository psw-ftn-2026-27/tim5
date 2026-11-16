using Payment.Domain.TourPurchaseTokens;

namespace Payment.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken PaidTourPurchasedByTouristWithoutCart = new(
        Guid.Parse("00000000-0000-0000-0007-000000000001"),
        ProductSeed.PaidTour.EntityId,
        new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc));

    public static TourPurchaseToken[] All => [PaidTourPurchasedByTouristWithoutCart];
}
