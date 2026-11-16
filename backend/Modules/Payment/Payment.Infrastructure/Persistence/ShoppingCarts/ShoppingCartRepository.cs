using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCarts;
using Payment.Domain.ShoppingCarts;

namespace Payment.Infrastructure.Persistence.ShoppingCarts;

internal sealed class ShoppingCartRepository : IShoppingCartRepository
{
    private readonly PaymentDbContext _dbContext;

    public ShoppingCartRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ShoppingCart?> GetByTouristAsync(Guid touristId)
    {
        return _dbContext.ShoppingCarts.FirstOrDefaultAsync(cart => cart.TouristId == touristId);
    }

    public void Add(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }
}
