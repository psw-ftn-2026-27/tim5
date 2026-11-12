using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Reversing;

public sealed class ReversingQueries
{
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IIdentityApi _identityApi;

    public ReversingQueries(IWalletReadRepository walletReadRepository, IIdentityApi identityApi)
    {
        _walletReadRepository = walletReadRepository;
        _identityApi = identityApi;
    }

    public async Task<TouristDepositsDto> GetTouristDepositsAsync(Guid touristId)
    {
        var users = await _identityApi.GetByIdsAsync([touristId]);
        if (users.Count == 0)
        {
            throw new NotFoundException($"User {touristId} does not exist.");
        }

        var wallet = await _walletReadRepository.GetDepositsAsync(touristId);

        return new TouristDepositsDto(users[0].Username, wallet?.Balance ?? 0, wallet?.Deposits ?? []);
    }
}
