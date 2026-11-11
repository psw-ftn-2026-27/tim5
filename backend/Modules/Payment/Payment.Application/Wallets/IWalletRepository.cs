using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByTouristAsync(Guid touristId);
    void Add(Wallet wallet);
}
