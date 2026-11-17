using Exploration.Contracts;
using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Application.TourPurchaseTokens;
using Payment.Application.Wallets;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.TourPurchaseTokens;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITourPurchaseTokenRepository _tourPurchaseTokenRepository;
    private readonly IExplorationApi _explorationApi;
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly BenefitCalculator _benefitCalculator;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingService(
        IShoppingCartRepository shoppingCartRepository,
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        ITourPurchaseTokenRepository tourPurchaseTokenRepository,
        IExplorationApi explorationApi,
        ICouponRepository couponRepository,
        IGiftCardRepository giftCardRepository,
        IPurchaseRepository purchaseRepository,
        BenefitCalculator benefitCalculator,
        IUnitOfWork unitOfWork)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _productRepository = productRepository;
        _walletRepository = walletRepository;
        _tourPurchaseTokenRepository = tourPurchaseTokenRepository;
        _explorationApi = explorationApi;
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
        _purchaseRepository = purchaseRepository;
        _benefitCalculator = benefitCalculator;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(Guid touristId, AddToCartDto dto)
    {
        var product = await _productRepository.GetByEntityAsync(dto.EntityId)
            ?? throw new NotFoundException($"Tour {dto.EntityId} has no product.");

        var token = await _tourPurchaseTokenRepository.GetByTouristAndEntityAsync(touristId, product.EntityId);

        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId);
        if (cart is null)
        {
            cart = new ShoppingCart(touristId);
            _shoppingCartRepository.Add(cart);
        }

        cart.AddItem(product.EntityId, product.Name, product.Price, product.OwnerId, token is not null);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid touristId, Guid entityId)
    {
        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId)
            ?? throw new NotFoundException($"Tourist {touristId} has no cart.");

        cart.RemoveItem(entityId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Guid> CheckoutAsync(Guid touristId, CheckoutBenefitsDto? benefits)
    {
        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId)
            ?? throw new Shared.Domain.Exceptions.DomainException("An empty cart cannot be paid.");
        var now = DateTime.UtcNow;
        var items = await GetCheckoutItemsAsync(cart);
        var couponCode = NormalizeCouponCode(benefits?.CouponCode);
        var coupon = couponCode is null ? null : await _couponRepository.GetByCodeAsync(couponCode);
        var giftCard = benefits?.GiftCardId is Guid giftCardId
            ? await _giftCardRepository.GetByIdAsync(giftCardId)
            : null;

        var tourIds = cart.Items.Select(item => item.EntityId).ToList();
        var publishedTourIds = await _explorationApi.GetPublishedAsync(tourIds);

        if (benefits?.GiftCardId is not null && giftCard is null)
        {
            throw new Shared.Domain.Exceptions.DomainException("The selected gift card does not exist.");
        }

        var calculation = _benefitCalculator.Calculate(items, coupon, couponCode, giftCard, touristId, now);
        if (calculation.CouponMessage is not null)
        {
            throw new Shared.Domain.Exceptions.DomainException(calculation.CouponMessage);
        }

        if (calculation.GiftCardMessage is not null)
        {
            throw new Shared.Domain.Exceptions.DomainException(calculation.GiftCardMessage);
        }

        // Validate availability and empty-cart rules before changing any tracked purchase state.
        var checkedOutItems = cart.Checkout(publishedTourIds);

        if (calculation.WalletItems.Count > 0)
        {
            var existingWallet = await _walletRepository.GetByTouristAsync(touristId);
            var wallet = existingWallet ?? new Wallet(touristId);
            wallet.Purchase(calculation.WalletItems, now);
            if (existingWallet is null)
            {
                _walletRepository.Add(wallet);
            }
        }

        if (calculation.GiftCardId is not null)
        {
            giftCard!.Redeem(touristId, now);
        }

        var purchase = new Purchase(
            touristId,
            calculation.Total,
            calculation.CouponId,
            calculation.CouponDiscount,
            calculation.GiftCardId,
            calculation.GiftCardDiscount,
            now);
        _purchaseRepository.Add(purchase);

        foreach (var item in checkedOutItems)
        {
            _tourPurchaseTokenRepository.Add(new TourPurchaseToken(touristId, item.EntityId, now));
        }

        await _unitOfWork.SaveChangesAsync();
        return purchase.Id;
    }

    private async Task<List<CheckoutItem>> GetCheckoutItemsAsync(ShoppingCart cart)
    {
        var items = new List<CheckoutItem>();
        foreach (var item in cart.Items)
        {
            var product = await _productRepository.GetByEntityAsync(item.EntityId)
                ?? throw new NotFoundException($"Tour {item.EntityId} has no product.");
            items.Add(new CheckoutItem(item.EntityId, item.Name, item.Price.Value, product.OwnerId));
        }

        return items;
    }

    internal static string? NormalizeCouponCode(string? couponCode)
    {
        return string.IsNullOrWhiteSpace(couponCode) ? null : couponCode.Trim();
    }
}