using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Depositing;
using Payment.Application.Listing;
using Payment.Application.Products;
using Payment.Application.Purchasing;
using Payment.Application.Reversing;
using Payment.Application.Shopping;
using Payment.Application.ShoppingCarts;
using Payment.Application.TourPurchaseTokens;
using Payment.Application.WalletReading;
using Payment.Application.Wallets;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.ShoppingCarts;
using Payment.Infrastructure.Persistence.TourPurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
        services.AddScoped<IShoppingCartReadRepository, ShoppingCartReadRepository>();
        services.AddScoped<ITourPurchaseTokenRepository, TourPurchaseTokenRepository>();
        services.AddScoped<ITourPurchaseTokenReadRepository, TourPurchaseTokenReadRepository>();

        services.AddScoped<WalletReadingQueries>();
        services.AddScoped<DepositingService>();
        services.AddScoped<DepositingQueries>();
        services.AddScoped<ReversingService>();
        services.AddScoped<ReversingQueries>();
        services.AddScoped<ListingService>();
        services.AddScoped<ListingQueries>();
        services.AddScoped<ShoppingService>();
        services.AddScoped<ShoppingQueries>();
        services.AddScoped<PurchasingQueries>();

        services.AddScoped<IPaymentApi, PaymentApi>();

        return services;
    }
}
