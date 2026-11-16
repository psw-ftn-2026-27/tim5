using Payment.Application.Listing;
using Payment.Application.Purchasing;
using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ListingService _listingService;
    private readonly ListingQueries _listingQueries;
    private readonly PurchasingQueries _purchasingQueries;

    public PaymentApi(ListingService listingService, ListingQueries listingQueries, PurchasingQueries purchasingQueries)
    {
        _listingService = listingService;
        _listingQueries = listingQueries;
        _purchasingQueries = purchasingQueries;
    }

    public Task ListProductAsync(Guid entityId, string name, Guid ownerId, int price)
    {
        return _listingService.ListAsync(entityId, name, ownerId, price);
    }

    public Task<IReadOnlyDictionary<Guid, int>> GetTourPricesAsync(IReadOnlyCollection<Guid> tourIds)
    {
        return _listingQueries.GetTourPricesAsync(tourIds);
    }

    public Task<bool> HasPurchasedAsync(Guid touristId, Guid tourId)
    {
        return _purchasingQueries.HasPurchasedAsync(touristId, tourId);
    }
}
