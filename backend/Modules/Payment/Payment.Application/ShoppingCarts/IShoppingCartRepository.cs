using Payment.Domain.ShoppingCarts;

namespace Payment.Application.ShoppingCarts;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetByTouristAsync(Guid touristId);
    void Add(ShoppingCart cart);
}
