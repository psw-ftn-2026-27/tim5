using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletReading;
using Payment.Application.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<WalletDto?> GetByTouristAsync(Guid touristId)
    {
        return _dbContext.Wallets
            .Where(wallet => wallet.TouristId == touristId)
            .Select(wallet => new WalletDto(
                wallet.Balance,
                wallet.Transactions
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new TransactionDto(transaction.Type, transaction.Amount, transaction.OccurredAt))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
