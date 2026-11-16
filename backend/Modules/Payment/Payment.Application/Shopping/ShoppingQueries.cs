using Payment.Application.Products;
using Payment.Application.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.Shopping;

public sealed class ShoppingQueries
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IShoppingCartReadRepository _shoppingCartReadRepository;

    public ShoppingQueries(IProductReadRepository productReadRepository, IShoppingCartReadRepository shoppingCartReadRepository)
    {
        _productReadRepository = productReadRepository;
        _shoppingCartReadRepository = shoppingCartReadRepository;
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
}
