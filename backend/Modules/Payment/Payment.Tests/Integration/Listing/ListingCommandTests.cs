using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Listing;

public class ListingCommandTests : BaseIntegrationTest
{
    public ListingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Listing_an_entity_without_a_product_creates_one_product_with_its_name_owner_and_price()
    {
        var entityId = Guid.Parse("00000000-0000-0000-0004-000000000003");
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        await paymentApi.ListProductAsync(entityId, "Fruskogorski manastiri", WellKnownUsers.Explorer, 300);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore + 1);
        var stored = assertContext.Products.Single(product => product.EntityId == entityId);
        stored.Name.Should().Be("Fruskogorski manastiri");
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Price.Value.Should().Be(300);
    }

    [Fact]
    public async Task Listing_an_entity_again_updates_its_name_and_price_and_keeps_the_original_owner()
    {
        var seeded = ProductSeed.PaidTour;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        await paymentApi.ListProductAsync(seeded.EntityId, "Tvrdjava nocu", WellKnownUsers.SecondExplorer, 220);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore);
        var stored = assertContext.Products.Single(product => product.EntityId == seeded.EntityId);
        stored.Name.Should().Be("Tvrdjava nocu");
        stored.Price.Value.Should().Be(220);
        stored.OwnerId.Should().Be(seeded.OwnerId);
    }

    [Fact]
    public async Task Listing_with_a_negative_price_is_rejected_and_neither_creates_nor_changes_a_product()
    {
        var seeded = ProductSeed.PaidTour;
        using var scope = Factory.Services.CreateScope();
        var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var productCountBefore = arrangeContext.Products.Count();

        var list = () => paymentApi.ListProductAsync(seeded.EntityId, "Tvrdjava nocu", seeded.OwnerId, -1);

        await list.Should().ThrowAsync<DomainException>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(productCountBefore);
        var stored = assertContext.Products.Single(product => product.EntityId == seeded.EntityId);
        stored.Name.Should().Be(seeded.Name);
        stored.Price.Value.Should().Be(seeded.Price.Value);
    }
}
