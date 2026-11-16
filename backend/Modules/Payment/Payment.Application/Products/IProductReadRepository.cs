using Payment.Application.Shopping;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, int>> GetPricesAsync(IReadOnlyCollection<Guid> entityIds);
    Task<ProductDto?> GetByEntityAsync(Guid entityId);
}
