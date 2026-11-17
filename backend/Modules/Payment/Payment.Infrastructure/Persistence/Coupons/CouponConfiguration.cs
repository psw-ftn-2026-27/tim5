using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal static class CouponConfiguration
{
    public static void ConfigureCoupons(this ModelBuilder modelBuilder)
    {
        ConfigureCoupon(modelBuilder.Entity<Coupon>());
    }

    private static void ConfigureCoupon(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasIndex(coupon => coupon.OwnerId);
        builder.HasIndex(coupon => coupon.ProductId);
    }
}
