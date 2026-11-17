using Shared.Domain;

namespace Payment.Domain.Purchases;

public sealed class Purchase : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public int Total { get; private set; }
    public Guid? CouponId { get; private set; }
    public int CouponDiscount { get; private set; }
    public Guid? GiftCardId { get; private set; }
    public int GiftCardDiscount { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private Purchase() { }

    public Purchase(
        Guid touristId,
        int total,
        Guid? couponId,
        int couponDiscount,
        Guid? giftCardId,
        int giftCardDiscount,
        DateTime purchasedAt) : base(Guid.NewGuid())
    {
        TouristId = touristId;
        Total = total;
        CouponId = couponId;
        CouponDiscount = couponDiscount;
        GiftCardId = giftCardId;
        GiftCardDiscount = giftCardDiscount;
        PurchasedAt = purchasedAt;
    }
}
