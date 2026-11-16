using Microsoft.EntityFrameworkCore;
using Payment.Application.Shopping;
using Payment.Application.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartReadRepository : IShoppingCartReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCartDto?> GetByTouristAsync(Guid touristId)
    {
        return _dbContext.ShoppingCarts
            .Where(cart => cart.TouristId == touristId)
            .Select(cart => new ShoppingCartDto(
                cart.Items
                    .Select(item => new OrderItemDto(item.EntityId, item.Name, item.Price.Value))
                    .ToList(),
                cart.Items.Sum(item => item.Price.Value)))
            .FirstOrDefaultAsync();
    }
}
