using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CouponCreating;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponCreating;

public class CouponCreatingCommandTests : BaseIntegrationTest
{
    private static DateOnly FutureExpiryDate => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

    public CouponCreatingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_explorer_creates_a_coupon_for_their_product_and_receives_its_id()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expiresAt = FutureExpiryDate;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, expiresAt, ProductSeed.PaidTour.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreatedCouponDto>(JsonOptions);
        created.Should().NotBeNull();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore + 1);
        var stored = assertContext.Coupons.Single(coupon => coupon.Id == created!.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.ProductId.Should().Be(ProductSeed.PaidTour.EntityId);
        stored.DiscountPercentage.Should().Be(20);
        stored.ExpiresAt.Should().Be(expiresAt);
        stored.Code.Should().HaveLength(8);
    }

    [Fact]
    public async Task An_explorer_creates_a_coupon_for_all_their_products_without_a_product_id()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var expiresAt = FutureExpiryDate;

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(15, expiresAt, null));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreatedCouponDto>(JsonOptions);
        created.Should().NotBeNull();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore + 1);
        var stored = assertContext.Coupons.Single(coupon => coupon.Id == created!.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.ProductId.Should().BeNull();
        stored.DiscountPercentage.Should().Be(15);
        stored.ExpiresAt.Should().Be(expiresAt);
        stored.Code.Should().HaveLength(8);
    }

    [Fact]
    public async Task An_explorer_cannot_create_a_coupon_for_another_owners_product()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, FutureExpiryDate, ProductSeed.PaidTour.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore);
    }

    [Fact]
    public async Task An_explorer_cannot_create_a_coupon_for_a_product_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var missingProductId = Guid.Parse("00000000-0000-0000-0004-000000000099");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, FutureExpiryDate, missingProductId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task An_explorer_cannot_create_a_coupon_with_an_invalid_discount_percentage(int discountPercentage)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(discountPercentage, FutureExpiryDate, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore);
    }

    [Fact]
    public async Task An_explorer_cannot_create_a_coupon_with_an_expiry_date_before_today()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var pastExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, pastExpiryDate, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore);
    }

    [Fact]
    public async Task An_explorer_cannot_create_a_coupon_with_a_fractional_discount_percentage()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var couponsBefore = arrangeContext.Coupons.Count();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(15.5m, FutureExpiryDate, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Coupons.Count().Should().Be(couponsBefore);
    }

    [Fact]
    public async Task An_anonymous_user_cannot_create_a_coupon()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, FutureExpiryDate, null));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_administrator_cannot_create_a_coupon()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/payment/coupons", new CreateCouponDto(20, FutureExpiryDate, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
