using Microsoft.EntityFrameworkCore;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletRepository : IWalletRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Wallet?> GetByTouristAsync(Guid touristId)
    {
        return _dbContext.Wallets.FirstOrDefaultAsync(wallet => wallet.TouristId == touristId);
    }

    public void Add(Wallet wallet)
    {
        _dbContext.Wallets.Add(wallet);
    }
}
