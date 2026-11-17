using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal static class PurchaseConfiguration
{
    public static void ConfigurePurchases(this ModelBuilder modelBuilder)
    {
        ConfigurePurchase(modelBuilder.Entity<Purchase>());
    }

    private static void ConfigurePurchase(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasIndex(purchase => purchase.TouristId);
    }
}
