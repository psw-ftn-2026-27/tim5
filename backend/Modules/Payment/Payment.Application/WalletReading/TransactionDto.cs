using Payment.Domain.Wallets;

namespace Payment.Application.WalletReading;

public sealed record TransactionDto(
    TransactionType Type,
    string? Name,
    int Amount,
    DateTime OccurredAt,
    bool IsReversed,
    DateTime? ReversedAt,
    Guid? GiftCardId);
