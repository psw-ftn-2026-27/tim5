namespace Payment.Domain.ShoppingCarts;

public sealed record CheckoutCalculation(
    int CartTotal,
    Guid? CouponId,
    int CouponDiscount,
    Guid? GiftCardId,
    int GiftCardDiscount,
    int Total,
    IReadOnlyList<Payment.Domain.Wallets.PurchaseItem> WalletItems,
    string? CouponMessage,
    string? GiftCardMessage);
