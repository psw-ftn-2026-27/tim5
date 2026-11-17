namespace Payment.Application.GiftCardPurchasing;

public sealed record PurchaseGiftCardDto(string RecipientUsername, int Amount, string Message);
