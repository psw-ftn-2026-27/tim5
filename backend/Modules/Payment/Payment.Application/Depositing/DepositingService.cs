using Identity.Contracts;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Depositing;

public sealed class DepositingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public DepositingService(IWalletRepository walletRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task DepositAsync(Guid touristId, Guid administratorId, DepositDto dto)
    {
        var amount = new Amount(dto.Amount);

        var users = await _identityApi.GetByIdsAsync([touristId]);
        if (users.Count == 0)
        {
            throw new NotFoundException($"User {touristId} does not exist.");
        }

        var wallet = await _walletRepository.GetByTouristAsync(touristId);
        if (wallet is null)
        {
            wallet = new Wallet(touristId);
            _walletRepository.Add(wallet);
        }

        wallet.Deposit(amount, administratorId);
        await _unitOfWork.SaveChangesAsync();
    }
}
