using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    private static readonly TimeSpan ReversalWindow = TimeSpan.FromHours(24);

    public TransactionType Type { get; private set; }
    public Amount Amount { get; private set; } = null!;
    public Guid? AdministratorId { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? Name { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? ReversedAt { get; private set; }
    public Guid? ReversedByAdministratorId { get; private set; }
    public bool IsReversed => ReversedAt is not null;

    private Transaction() { }

    public Transaction(TransactionType type, Amount amount, Guid? administratorId, Guid? entityId, string? name, DateTime occurredAt)
        : base(Guid.NewGuid())
    {
        if (type == TransactionType.Deposit && administratorId is null)
        {
            throw new DomainException("A deposit must record the administrator who made it.");
        }

        if (type == TransactionType.Purchase && (entityId is null || string.IsNullOrWhiteSpace(name)))
        {
            throw new DomainException("A purchase must record the purchased tour and its name.");
        }

        Type = type;
        Amount = amount;
        AdministratorId = administratorId;
        EntityId = entityId;
        Name = name;
        OccurredAt = occurredAt;
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
