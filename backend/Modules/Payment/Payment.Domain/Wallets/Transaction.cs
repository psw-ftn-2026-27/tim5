using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    private static readonly TimeSpan ReversalWindow = TimeSpan.FromHours(24);

    public TransactionType Type { get; private set; }
    public Amount Amount { get; private set; } = null!;
    public Guid AdministratorId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public Guid? ReversedByAdministratorId { get; private set; }
    public bool IsReversed => ReversedAt is not null;

    private Transaction() { }

    public Transaction(TransactionType type, Amount amount, Guid administratorId) : base(Guid.NewGuid())
    {
        Type = type;
        Amount = amount;
        AdministratorId = administratorId;
        OccurredAt = DateTime.UtcNow;
    }

    internal void Reverse(Guid administratorId, DateTime now)
    {
        if (Type != TransactionType.Deposit)
        {
            throw new DomainException("Only a deposit can be reversed.");
        }

        if (IsReversed)
        {
            throw new DomainException("The deposit has already been reversed.");
        }

        if (now - OccurredAt > ReversalWindow)
        {
            throw new DomainException("A deposit can be reversed only within 24 hours.");
        }

        ReversedAt = now;
        ReversedByAdministratorId = administratorId;
    }
}
