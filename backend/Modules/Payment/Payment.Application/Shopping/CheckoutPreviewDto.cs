namespace Payment.Application.Shopping;

public sealed record CheckoutPreviewDto(
    int CartTotal,
    Guid? CouponId,
    string? CouponCode,
    int CouponDiscount,
    Guid? GiftCardId,
    int GiftCardDiscount,
    int Total,
    string? CouponMessage,
    string? GiftCardMessage);
