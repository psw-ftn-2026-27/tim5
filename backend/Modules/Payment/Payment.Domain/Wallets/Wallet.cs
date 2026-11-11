using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public int Balance { get; private set; }
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Wallet() { }

    public Wallet(Guid touristId) : base(Guid.NewGuid())
    {
        TouristId = touristId;
        Balance = 0;
    }

    public void Deposit(Amount amount, Guid administratorId)
    {
        var transaction = new Transaction(TransactionType.Deposit, amount, administratorId);

        _transactions.Add(transaction);
        Balance += transaction.Amount.Value;
    }

    public void Reverse(Guid transactionId, Guid administratorId, DateTime now)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId)
            ?? throw new NotFoundException($"Deposit {transactionId} does not exist.");

        if (Balance - transaction.Amount.Value < 0)
        {
            throw new DomainException("The reversal would make the balance negative.");
        }

        transaction.Reverse(administratorId, now);
        Balance -= transaction.Amount.Value;
    }
}
