using Identity.Contracts;
using Payment.Application.Wallets;

namespace Payment.Application.Depositing;

public sealed class DepositingQueries
{
    private const int SearchLimit = 20;

    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public DepositingQueries(IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
        _walletReadRepository = walletReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<TouristBalanceDto>> GetTouristBalancesAsync(string username)
    {
        var users = await _identityApi.SearchByUsernameAsync(username, SearchLimit);
        var balances = await _walletReadRepository.GetBalancesAsync(users.Select(user => user.UserId).ToList());

        return users
            .Select(user => new TouristBalanceDto(
                user.UserId,
                user.Username,
                balances.FirstOrDefault(balance => balance.TouristId == user.UserId)?.Balance ?? 0))
            .ToList();
    }
}
