using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCardPurchasing;

public sealed class GiftCardPurchasingService
{
    private const int SearchLimit = 20;

    private readonly IIdentityApi _identityApi;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GiftCardPurchasingService(
        IIdentityApi identityApi,
        IGiftCardRepository giftCardRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork)
    {
        _identityApi = identityApi;
        _giftCardRepository = giftCardRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> PurchaseGiftCardAsync(Guid senderId, PurchaseGiftCardDto dto)
    {
        var recipients = await _identityApi.SearchByUsernameAsync(dto.RecipientUsername, SearchLimit);
        var recipient = recipients.FirstOrDefault(user =>
            string.Equals(user.Username, dto.RecipientUsername, StringComparison.OrdinalIgnoreCase))
            ?? throw new NotFoundException($"User {dto.RecipientUsername} does not exist.");

        var now = DateTime.UtcNow;
        var giftCard = new GiftCard(senderId, recipient.UserId, new Amount(dto.Amount), dto.Message, now);
        var wallet = await _walletRepository.GetByTouristAsync(senderId);
        var newWallet = wallet is null;
        wallet ??= new Wallet(senderId);
        wallet.PurchaseGiftCard(giftCard.Id, giftCard.Amount, now);

        if (newWallet)
        {
            _walletRepository.Add(wallet);
        }

        _giftCardRepository.Add(giftCard);
        await _unitOfWork.SaveChangesAsync();

        return giftCard.Id;
    }
}
