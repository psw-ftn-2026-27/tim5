using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.Products;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.TourPurchaseTokens;
using Payment.Domain.Wallets;
using Payment.Domain.Purchases;
using Payment.Infrastructure.Persistence.Purchases;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Coupons;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<TourPurchaseToken> TourPurchaseTokens => Set<TourPurchaseToken>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureGiftCards();
        builder.ConfigureProducts();
        builder.ConfigureCoupons();
        builder.ConfigureShoppingCarts();
        builder.ConfigureTourPurchaseTokens();
        builder.ConfigurePurchases();
    }
}
