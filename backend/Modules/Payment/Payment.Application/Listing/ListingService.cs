using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Application.Listing;

public sealed class ListingService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ListingService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ListAsync(Guid entityId, string name, Guid ownerId, int price)
    {
        var listingPrice = new Price(price);

        var product = await _productRepository.GetByEntityAsync(entityId);
        if (product is null)
        {
            product = new Product(entityId, name, ownerId, listingPrice);
            _productRepository.Add(product);
        }
        else
        {
            product.UpdateListing(name, listingPrice);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
