using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductReadRepository : IProductReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Dictionary<Guid, int>> GetPricesAsync(IReadOnlyCollection<Guid> entityIds)
    {
        return _dbContext.Products
            .Where(product => entityIds.Contains(product.EntityId))
            .Select(product => new { product.EntityId, Price = product.Price.Value })
            .ToDictionaryAsync(product => product.EntityId, product => product.Price);
    }
}
