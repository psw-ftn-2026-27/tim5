using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Application.Shopping;

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

    public Task<ProductDto?> GetByEntityAsync(Guid entityId)
    {
        return _dbContext.Products
            .Where(product => product.EntityId == entityId)
            .Select(product => new ProductDto(product.Name, product.Price.Value))
            .FirstOrDefaultAsync();
    }
}
