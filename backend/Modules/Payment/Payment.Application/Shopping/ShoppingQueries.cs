using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Products;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingQueries
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IShoppingCartReadRepository _shoppingCartReadRepository;
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IPurchaseReadRepository _purchaseReadRepository;
    private readonly BenefitCalculator _benefitCalculator;

    public ShoppingQueries(
        IProductReadRepository productReadRepository,
        IShoppingCartReadRepository shoppingCartReadRepository,
        IShoppingCartRepository shoppingCartRepository,
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        IGiftCardRepository giftCardRepository,
        IPurchaseReadRepository purchaseReadRepository,
        BenefitCalculator benefitCalculator)
    {
        _productReadRepository = productReadRepository;
        _shoppingCartReadRepository = shoppingCartReadRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _giftCardRepository = giftCardRepository;
        _purchaseReadRepository = purchaseReadRepository;
        _benefitCalculator = benefitCalculator;
    }

    public async Task<ProductDto> GetProductAsync(Guid entityId)
    {
        return await _productReadRepository.GetByEntityAsync(entityId)
            ?? throw new NotFoundException($"Tour {entityId} has no product.");
    }

    public async Task<ShoppingCartDto> GetMyCartAsync(Guid touristId)
    {
        var cart = await _shoppingCartReadRepository.GetByTouristAsync(touristId);
        return cart ?? new ShoppingCartDto([], 0);
    }

    public async Task<CheckoutPreviewDto> GetCheckoutPreviewAsync(Guid touristId, string? couponCode, Guid? giftCardId)
    {
        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId);
        var items = cart is null ? [] : await GetCheckoutItemsAsync(cart);
        var normalizedCode = ShoppingService.NormalizeCouponCode(couponCode);
        var coupon = normalizedCode is null ? null : await _couponRepository.GetByCodeAsync(normalizedCode);
        var giftCard = giftCardId is Guid id ? await _giftCardRepository.GetByIdAsync(id) : null;
        var calculation = _benefitCalculator.Calculate(items, coupon, normalizedCode, giftCard, touristId, DateTime.UtcNow);

        return new CheckoutPreviewDto(
            calculation.CartTotal,
            calculation.CouponId,
            coupon?.Code,
            calculation.CouponDiscount,
            calculation.GiftCardId,
            calculation.GiftCardDiscount,
            calculation.Total,
            calculation.CouponMessage,
            calculation.GiftCardMessage);
    }

    public async Task<PurchaseDto> GetPurchaseAsync(Guid id, Guid touristId)
    {
        return await _purchaseReadRepository.GetByIdForTouristAsync(id, touristId)
            ?? throw new NotFoundException($"Purchase {id} does not exist.");
    }

    private async Task<List<CheckoutItem>> GetCheckoutItemsAsync(Payment.Domain.ShoppingCarts.ShoppingCart cart)
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
}
