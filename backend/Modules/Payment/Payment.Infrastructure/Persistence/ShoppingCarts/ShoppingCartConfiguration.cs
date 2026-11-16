using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal static class ShoppingCartConfiguration
{
    public static void ConfigureShoppingCarts(this ModelBuilder modelBuilder)
    {
        ConfigureShoppingCart(modelBuilder.Entity<ShoppingCart>());
        ConfigureOrderItem(modelBuilder.Entity<OrderItem>());
    }

    private static void ConfigureShoppingCart(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.HasIndex(cart => cart.TouristId).IsUnique();

        builder.HasMany(cart => cart.Items)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(cart => cart.Items).AutoInclude();
    }

    private static void ConfigureOrderItem(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.ComplexProperty(item => item.Price, price =>
        {
            price.Property(value => value.Value).HasColumnName("Price");
        });
    }
}
