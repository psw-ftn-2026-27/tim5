using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByEntityAsync(Guid entityId);
    void Add(Product product);
}
