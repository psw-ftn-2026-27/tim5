using Payment.Application.Products;

namespace Payment.Application.Listing;

public sealed class ListingQueries
{
    private readonly IProductReadRepository _productReadRepository;

    public ListingQueries(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetTourPricesAsync(IReadOnlyCollection<Guid> tourIds)
    {
        return await _productReadRepository.GetPricesAsync(tourIds);
    }

    public async Task<IReadOnlyList<MyProductDto>> GetMyProductsAsync(Guid ownerId)
    {
        return await _productReadRepository.GetMyProductsAsync(ownerId);
    }
}
