using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public TransactionType Type { get; private set; }
    public Amount Amount { get; private set; } = null!;
    public Guid AdministratorId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private Transaction() { }

    public Transaction(TransactionType type, Amount amount, Guid administratorId) : base(Guid.NewGuid())
    {
        Type = type;
        Amount = amount;
        AdministratorId = administratorId;
        OccurredAt = DateTime.UtcNow;
    }
}
