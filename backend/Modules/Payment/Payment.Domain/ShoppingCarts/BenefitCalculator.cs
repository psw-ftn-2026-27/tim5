using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;

namespace Payment.Domain.ShoppingCarts;

public sealed class BenefitCalculator
{
    public CheckoutCalculation Calculate(
        IReadOnlyList<CheckoutItem> items,
        Coupon? coupon,
        string? couponCode,
        GiftCard? giftCard,
        Guid customerId,
        DateTime now)
    {
        var cartTotal = items.Sum(item => item.Price);
        var adjustedPrices = items.Select(item => item.Price).ToArray();
        var couponDiscount = 0;
        Guid? appliedCouponId = null;
        string? couponMessage = null;

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            if (coupon is null)
            {
                couponMessage = "The coupon code is invalid.";
            }
            else if (!coupon.IsValidAt(now))
            {
                couponMessage = "The coupon has expired.";
            }
            else
            {
                var targetIndex = FindTargetItem(items, coupon);
                if (targetIndex < 0)
                {
                    couponMessage = "The coupon does not apply to any product in the cart.";
                }
                else
                {
                    couponDiscount = (int)Math.Floor(adjustedPrices[targetIndex] * coupon.DiscountPercentage / 100m);
                    adjustedPrices[targetIndex] -= couponDiscount;
                    appliedCouponId = coupon.Id;
                }
            }
        }

        var giftCardDiscount = 0;
        Guid? appliedGiftCardId = null;
        string? giftCardMessage = null;
        if (giftCard is not null)
        {
            if (giftCard.RecipientId != customerId)
            {
                giftCardMessage = "The gift card does not belong to this user.";
            }
            else if (giftCard.GetStatus(now) != GiftCardStatus.Valid)
            {
                giftCardMessage = "The gift card is expired or has already been redeemed.";
            }
            else
            {
                giftCardDiscount = Math.Min(giftCard.Amount.Value, adjustedPrices.Sum());
                ApplyGiftCard(adjustedPrices, giftCard.Amount.Value);
                appliedGiftCardId = giftCard.Id;
            }
        }

        var total = adjustedPrices.Sum();
        var walletItems = items
            .Select((item, index) => new { item, price = adjustedPrices[index] })
            .Where(line => line.price > 0 || line.item.Price == 0)
            .Select(line => new PurchaseItem(line.item.EntityId, line.item.Name, new Amount(line.price)))
            .ToList();

        return new CheckoutCalculation(
            cartTotal,
            appliedCouponId,
            couponDiscount,
            appliedGiftCardId,
            giftCardDiscount,
            total,
            walletItems,
            couponMessage,
            giftCardMessage);
    }

    private static int FindTargetItem(IReadOnlyList<CheckoutItem> items, Coupon coupon)
    {
        if (coupon.ProductId is not null)
        {
            for (var index = 0; index < items.Count; index++)
            {
                if (coupon.AppliesTo(items[index].EntityId, items[index].OwnerId))
                {
                    return index;
                }
            }

            return -1;
        }

        return items
            .Select((item, index) => new { item, index })
            .Where(line => coupon.AppliesTo(line.item.EntityId, line.item.OwnerId))
            .OrderByDescending(line => line.item.Price)
            .Select(line => line.index)
            .FirstOrDefault(-1);
    }

    private static void ApplyGiftCard(int[] prices, int amount)
    {
        var remaining = amount;
        for (var index = 0; index < prices.Length && remaining > 0; index++)
        {
            var applied = Math.Min(prices[index], remaining);
            prices[index] -= applied;
            remaining -= applied;
        }
    }
}
