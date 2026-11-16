using Payment.Domain.TourPurchaseTokens;

namespace Payment.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken PaidTourPurchasedByTouristWithoutCart = new(
        Guid.Parse("00000000-0000-0000-0007-000000000001"),
        ProductSeed.PaidTour.EntityId,
        new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken PaidTourPurchasedFirstByTouristWithTwoPurchases = new(
        WalletSeed.WalletWithTwoPurchases.TouristId,
        ProductSeed.PaidTour.EntityId,
        new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken SecondPaidTourPurchasedLaterByTouristWithTwoPurchases = new(
        WalletSeed.WalletWithTwoPurchases.TouristId,
        ProductSeed.SecondPaidTour.EntityId,
        new DateTime(2026, 11, 12, 9, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken PaidTourPurchasedByAnotherTourist = new(
        WalletSeed.AnotherTouristsWalletWithOnePurchase.TouristId,
        ProductSeed.PaidTour.EntityId,
        new DateTime(2026, 11, 11, 9, 0, 0, DateTimeKind.Utc));

    public static TourPurchaseToken[] All =>
        [PaidTourPurchasedByTouristWithoutCart, PaidTourPurchasedFirstByTouristWithTwoPurchases,
            SecondPaidTourPurchasedLaterByTouristWithTwoPurchases, PaidTourPurchasedByAnotherTourist];
}
