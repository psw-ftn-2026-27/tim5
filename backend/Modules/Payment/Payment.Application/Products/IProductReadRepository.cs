using Payment.Application.Shopping;
using Payment.Application.Listing;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<Dictionary<Guid, int>> GetPricesAsync(IReadOnlyCollection<Guid> entityIds);
    Task<ProductDto?> GetByEntityAsync(Guid entityId);
    Task<IReadOnlyList<MyProductDto>> GetMyProductsAsync(Guid ownerId);
}
