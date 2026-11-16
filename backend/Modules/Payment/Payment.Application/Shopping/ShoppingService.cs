using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShoppingService(IShoppingCartRepository shoppingCartRepository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(Guid touristId, AddToCartDto dto)
    {
        var product = await _productRepository.GetByEntityAsync(dto.EntityId)
            ?? throw new NotFoundException($"Tour {dto.EntityId} has no product.");

        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId);
        if (cart is null)
        {
            cart = new ShoppingCart(touristId);
            _shoppingCartRepository.Add(cart);
        }

        cart.AddItem(product.EntityId, product.Name, product.Price, product.OwnerId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromCartAsync(Guid touristId, Guid entityId)
    {
        var cart = await _shoppingCartRepository.GetByTouristAsync(touristId)
            ?? throw new NotFoundException($"Tourist {touristId} has no cart.");

        cart.RemoveItem(entityId);
        await _unitOfWork.SaveChangesAsync();
    }
}
