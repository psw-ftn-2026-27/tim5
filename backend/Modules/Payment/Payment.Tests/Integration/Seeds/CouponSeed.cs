using Payment.Domain.Coupons;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    public static readonly Coupon ExplorersCoupon = new(
        "AB12CD34",
        WellKnownUsers.Explorer,
        ProductSeed.PaidTour.EntityId,
        15,
        new DateOnly(2027, 1, 15),
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static Coupon[] All => [ExplorersCoupon];
}
