using Payment.Application.Wallets;

namespace Payment.Application.WalletReading;

public sealed class WalletReadingQueries
{
    private readonly IWalletReadRepository _walletReadRepository;

    public WalletReadingQueries(IWalletReadRepository walletReadRepository)
    {
        _walletReadRepository = walletReadRepository;
    }

    public async Task<WalletDto> GetMineAsync(Guid touristId)
    {
        var wallet = await _walletReadRepository.GetByTouristAsync(touristId);

        return wallet ?? new WalletDto(0, []);
    }
}
