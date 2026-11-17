using Payment.Domain.Purchases;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class PurchaseSeed
{
    public static readonly Purchase PurchaseOfExplorer = new(
        WellKnownUsers.Explorer,
        150,
        CouponSeed.ExplorersCoupon.Id,
        25,
        GiftCardSeed.RecentValidCardReceivedByExplorer.Id,
        100,
        new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc));

    public static Purchase[] All => [PurchaseOfExplorer];
}
