using Payment.Application.Shopping;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartReadRepository
{
    Task<ShoppingCartDto?> GetByTouristAsync(Guid touristId);
}
