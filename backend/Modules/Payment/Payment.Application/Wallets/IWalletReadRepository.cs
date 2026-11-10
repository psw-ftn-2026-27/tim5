using Payment.Application.WalletReading;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<WalletDto?> GetByTouristAsync(Guid touristId);
}
