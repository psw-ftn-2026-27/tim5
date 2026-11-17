using Exploration.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public PaymentModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment())
        {
            await SeedDemoProductsAsync(dbContext);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    // Products for the demo tours seeded by Exploration. A product that already exists is
    // left as it is, so a listing published through the application is never overwritten.
    // The city center walk deliberately gets no product.
    // The names and prices are copied from ExplorationModuleInitializer.CreateDemoTours,
    // which is the source of truth: when a demo tour's name or price changes there, it must
    // be changed here as well.
    private static async Task SeedDemoProductsAsync(PaymentDbContext dbContext)
    {
        var fortress = WellKnownTours.PetrovaradinFortressWalk;
        if (!await dbContext.Products.AnyAsync(p => p.EntityId == fortress))
        {
            dbContext.Products.Add(new Product(fortress, "Šetnja Petrovaradinskom tvrđavom", FirstExplorer, new Price(300)));
        }

        var fruskaGora = WellKnownTours.FruskaGoraBikeRide;
        if (!await dbContext.Products.AnyAsync(p => p.EntityId == fruskaGora))
        {
            dbContext.Products.Add(new Product(fruskaGora, "Biciklom preko Fruške gore", FirstExplorer, new Price(0)));
        }

        await dbContext.SaveChangesAsync();
    }
}
