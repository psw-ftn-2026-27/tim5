using Payment.Application.Listing;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ListingService _listingService;
    private readonly ListingQueries _listingQueries;

    public PaymentApi(ListingService listingService, ListingQueries listingQueries)
    {
        _listingService = listingService;
        _listingQueries = listingQueries;
    }

    public Task ListProductAsync(Guid entityId, string name, Guid ownerId, int price)
    {
        return _listingService.ListAsync(entityId, name, ownerId, price);
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetTourPricesAsync(IReadOnlyCollection<Guid> tourIds)
    {
        return _listingQueries.GetTourPricesAsync(tourIds);
    }
}
