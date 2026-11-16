using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Payment.Application.TourPurchaseTokens;
using Payment.Application.Wallets;
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
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingService(
        IShoppingCartRepository shoppingCartRepository,
        IProductRepository productRepository,
        IWalletRepository walletRepository,
        ITourPurchaseTokenRepository tourPurchaseTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _productRepository = productRepository;
        _walletRepository = walletRepository;
        _tourPurchaseTokenRepository = tourPurchaseTokenRepository;
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

    public async Task CheckoutAsync(Guid touristId)
    {
        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId) ?? new ShoppingCart(touristId);
        var wallet = await _walletRepository.GetByTouristAsync(touristId) ?? new Wallet(touristId);
        var now = DateTime.UtcNow;

        var items = cart.Checkout();
        var purchaseItems = items
            .Select(item => new PurchaseItem(item.EntityId, item.Name, new Amount(item.Price.Value)))
            .ToList();
        wallet.Purchase(purchaseItems, now);
        foreach (var item in items)
        {
            _tourPurchaseTokenRepository.Add(new TourPurchaseToken(touristId, item.EntityId, now));
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
