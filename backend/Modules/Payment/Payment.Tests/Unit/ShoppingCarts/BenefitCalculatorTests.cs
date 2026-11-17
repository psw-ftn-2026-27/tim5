using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class BenefitCalculatorTests
{
    private static readonly Guid OwnerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid CustomerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly DateTime Now = new(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void An_owner_coupon_discounts_the_most_expensive_owned_item_and_floors_the_discount()
    {
        var coupon = new Coupon("OWNER20X", OwnerId, null, 20, new DateOnly(2026, 11, 17), Now);
        var items = new List<CheckoutItem>
        {
            new(Guid.NewGuid(), "First", 100, OwnerId),
            new(Guid.NewGuid(), "Most expensive", 301, OwnerId),
            new(Guid.NewGuid(), "Other owner", 1000, Guid.NewGuid())
        };

        var calculation = new BenefitCalculator().Calculate(items, coupon, coupon.Code, null, CustomerId, Now);

        calculation.CouponDiscount.Should().Be(60);
        calculation.Total.Should().Be(1341);
        calculation.WalletItems.Select(item => item.Price.Value).Should().BeEquivalentTo([100, 241, 1000]);
    }

    [Fact]
    public void A_product_coupon_only_discounts_its_product()
    {
        var productId = Guid.NewGuid();
        var coupon = new Coupon("PRODUCT1", OwnerId, productId, 50, new DateOnly(2026, 11, 17), Now);
        var items = new List<CheckoutItem>
        {
            new(productId, "Owned", 200, OwnerId),
            new(Guid.NewGuid(), "Other", 300, OwnerId)
        };

        var calculation = new BenefitCalculator().Calculate(items, coupon, coupon.Code, null, CustomerId, Now);

        calculation.CouponDiscount.Should().Be(100);
        calculation.Total.Should().Be(400);
    }

    [Fact]
    public void A_coupon_is_valid_through_its_expiry_date_in_utc()
    {
        var coupon = new Coupon("TODAY123", OwnerId, null, 10, new DateOnly(2026, 11, 17), Now);
        var items = new List<CheckoutItem> { new(Guid.NewGuid(), "Owned", 100, OwnerId) };

        var calculation = new BenefitCalculator().Calculate(
            items, coupon, coupon.Code, null, CustomerId, new DateTime(2026, 11, 17, 23, 59, 59, DateTimeKind.Utc));

        calculation.CouponId.Should().Be(coupon.Id);
        calculation.CouponDiscount.Should().Be(10);
    }

    [Fact]
    public void An_expired_or_inapplicable_coupon_is_reported_without_changing_the_cart_total()
    {
        var expiredCoupon = new Coupon("EXPIRED1", OwnerId, null, 10, new DateOnly(2026, 11, 16), new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc));
        var items = new List<CheckoutItem> { new(Guid.NewGuid(), "Owned", 100, OwnerId) };

        var calculation = new BenefitCalculator().Calculate(items, expiredCoupon, expiredCoupon.Code, null, CustomerId, Now);

        calculation.CouponMessage.Should().NotBeNullOrWhiteSpace();
        calculation.CouponDiscount.Should().Be(0);
        calculation.Total.Should().Be(100);
    }

    [Fact]
    public void A_gift_card_is_fully_redeemed_and_the_checkout_total_cannot_fall_below_zero()
    {
        var card = new GiftCard(Guid.NewGuid(), CustomerId, new Amount(500), "Gift", Now);
        var items = new List<CheckoutItem> { new(Guid.NewGuid(), "Owned", 100, OwnerId) };

        var calculation = new BenefitCalculator().Calculate(items, null, null, card, CustomerId, Now);

        calculation.GiftCardId.Should().Be(card.Id);
        calculation.GiftCardDiscount.Should().Be(100);
        calculation.Total.Should().Be(0);
        calculation.WalletItems.Should().BeEmpty();
    }
}
