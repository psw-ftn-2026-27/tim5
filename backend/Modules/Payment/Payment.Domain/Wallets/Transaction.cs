using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public TransactionType Type { get; private set; }
    public int Amount { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private Transaction() { }

    public Transaction(TransactionType type, int amount) : base(Guid.NewGuid())
    {
        Type = type;
        Amount = amount;
        OccurredAt = DateTime.UtcNow;
    }
}
