namespace Payment.Application.Shopping;

public sealed record PurchaseDto(
    Guid Id,
    int Total,
    Guid? CouponId,
    int CouponDiscount,
    Guid? GiftCardId,
    int GiftCardDiscount,
    DateTime PurchasedAt);
