using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Depositing;
using Payment.Application.Reversing;
using Payment.Application.WalletReading;
using Payment.Application.Wallets;
using Payment.Infrastructure.Persistence;
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

        services.AddScoped<WalletReadingQueries>();
        services.AddScoped<DepositingService>();
        services.AddScoped<DepositingQueries>();
        services.AddScoped<ReversingService>();
        services.AddScoped<ReversingQueries>();

        return services;
    }
}
