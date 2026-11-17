using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCardReading;

public sealed record GiftCardDto(
    Guid Id,
    string? OtherPartyUsername,
    int Amount,
    string Message,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    GiftCardStatus Status);
