using Payment.Domain.Wallets;

namespace Payment.Application.WalletReading;

public sealed record TransactionDto(TransactionType Type, int Amount, DateTime OccurredAt, bool IsReversed, DateTime? ReversedAt);
