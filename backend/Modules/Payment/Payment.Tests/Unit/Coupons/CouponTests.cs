using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_coupon_code_must_contain_eight_characters()
    {
        var create = () => new Coupon("ABC1234", Guid.NewGuid(), null, 25, new DateOnly(2026, 11, 17), CreatedAt);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void A_coupon_accepts_whole_discount_percentages_within_the_inclusive_range(int discountPercentage)
    {
        var coupon = CreateCoupon(discountPercentage, new DateOnly(2026, 11, 17));

        coupon.DiscountPercentage.Should().Be(discountPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void A_coupon_rejects_discount_percentages_outside_the_inclusive_range(int discountPercentage)
    {
        var create = () => CreateCoupon(discountPercentage, new DateOnly(2026, 11, 17));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_rejects_a_discount_percentage_with_a_fractional_part()
    {
        var create = () => CreateCoupon(15.5m, new DateOnly(2026, 11, 17));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_rejects_an_expiry_date_before_its_creation_date()
    {
        var create = () => CreateCoupon(25, new DateOnly(2026, 11, 16));

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void A_coupon_accepts_an_expiry_date_on_or_after_its_creation_date(int daysAfterCreation)
    {
        var coupon = CreateCoupon(25, DateOnly.FromDateTime(CreatedAt).AddDays(daysAfterCreation));

        coupon.ExpiresAt.Should().Be(DateOnly.FromDateTime(CreatedAt).AddDays(daysAfterCreation));
    }

    private static Coupon CreateCoupon(decimal discountPercentage, DateOnly expiresAt)
    {
        return new Coupon("AB12CD34", Guid.NewGuid(), null, discountPercentage, expiresAt, CreatedAt);
    }
}
