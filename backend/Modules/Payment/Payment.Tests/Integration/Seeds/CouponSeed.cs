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

    public static readonly Coupon CartWideTwentyPercentCoupon = new(
        "ALL20XYZ",
        WellKnownUsers.Explorer,
        null,
        20,
        new DateOnly(2027, 1, 15),
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly Coupon CartWideFullDiscountCoupon = new(
        "FULL100X",
        WellKnownUsers.Explorer,
        null,
        100,
        new DateOnly(2027, 1, 15),
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly Coupon ExpiredCoupon = new(
        "OLD00001",
        WellKnownUsers.Explorer,
        null,
        20,
        new DateOnly(2026, 11, 16),
        new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));

    public static Coupon[] All => [ExplorersCoupon, CartWideTwentyPercentCoupon, CartWideFullDiscountCoupon, ExpiredCoupon];
}
