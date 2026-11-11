using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Reversing;

public sealed class ReversingService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReversingService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ReverseAsync(Guid touristId, Guid depositId, Guid administratorId)
    {
        var wallet = await _walletRepository.GetByTouristAsync(touristId)
            ?? throw new NotFoundException($"Tourist {touristId} has no wallet.");

        wallet.Reverse(depositId, administratorId, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
