using Payment.Application.Depositing;
using Payment.Application.Reversing;
using Payment.Application.WalletReading;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<WalletDto?> GetByTouristAsync(Guid touristId);
    Task<List<WalletBalanceDto>> GetBalancesAsync(List<Guid> touristIds);
    Task<WalletDepositsDto?> GetDepositsAsync(Guid touristId);
}
