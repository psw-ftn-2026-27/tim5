using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CouponCreating;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponCreating;

public class CouponCreatingQueryTests : BaseIntegrationTest
{
    public CouponCreatingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_coupon_owner_can_read_their_coupon_details()
    {
        var coupon = CouponSeed.ExplorersCoupon;
        var client = Factory.CreateClientFor(coupon.OwnerId, "explorer");

        var result = await client.GetFromJsonAsync<CouponDto>($"/api/payment/coupons/{coupon.Id}", JsonOptions);

        result.Should().BeEquivalentTo(new CouponDto(
            coupon.Id,
            coupon.Code,
            coupon.DiscountPercentage,
            coupon.ExpiresAt,
            coupon.ProductId));
    }

    [Fact]
    public async Task Another_explorer_cannot_read_a_coupon_they_do_not_own()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/payment/coupons/{CouponSeed.ExplorersCoupon.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task An_anonymous_user_cannot_read_coupon_details()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/payment/coupons/{CouponSeed.ExplorersCoupon.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_administrator_cannot_read_explorer_coupon_details()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/payment/coupons/{CouponSeed.ExplorersCoupon.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
