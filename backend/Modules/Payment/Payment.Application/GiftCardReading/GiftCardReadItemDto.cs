namespace Payment.Application.GiftCardReading;

public sealed record GiftCardReadItemDto(
    Guid Id,
    Guid OtherUserId,
    int Amount,
    string Message,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? RedeemedAt);
