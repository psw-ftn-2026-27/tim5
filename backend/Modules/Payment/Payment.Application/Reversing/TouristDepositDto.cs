namespace Payment.Application.Reversing;

public sealed record TouristDepositDto(Guid Id, int Amount, DateTime OccurredAt, bool IsReversed, DateTime? ReversedAt);
