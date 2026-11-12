using Microsoft.EntityFrameworkCore;
using Payment.Application.Depositing;
using Payment.Application.Reversing;
using Payment.Application.WalletReading;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;

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
                    .Select(transaction => new TransactionDto(
                        transaction.Type,
                        transaction.Amount.Value,
                        transaction.OccurredAt,
                        transaction.ReversedAt != null,
                        transaction.ReversedAt))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public Task<List<WalletBalanceDto>> GetBalancesAsync(List<Guid> touristIds)
    {
        return _dbContext.Wallets
            .Where(wallet => touristIds.Contains(wallet.TouristId))
            .Select(wallet => new WalletBalanceDto(wallet.TouristId, wallet.Balance))
            .ToListAsync();
    }

    public Task<WalletDepositsDto?> GetDepositsAsync(Guid touristId)
    {
        return _dbContext.Wallets
            .Where(wallet => wallet.TouristId == touristId)
            .Select(wallet => new WalletDepositsDto(
                wallet.Balance,
                wallet.Transactions
                    .Where(transaction => transaction.Type == TransactionType.Deposit)
                    .OrderByDescending(transaction => transaction.OccurredAt)
                    .Select(transaction => new TouristDepositDto(
                        transaction.Id,
                        transaction.Amount.Value,
                        transaction.OccurredAt,
                        transaction.ReversedAt != null,
                        transaction.ReversedAt))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
