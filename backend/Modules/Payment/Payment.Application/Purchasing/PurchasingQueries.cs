using Payment.Application.TourPurchaseTokens;

namespace Payment.Application.Purchasing;

public sealed class PurchasingQueries
{
    private readonly ITourPurchaseTokenReadRepository _tourPurchaseTokenReadRepository;

    public PurchasingQueries(ITourPurchaseTokenReadRepository tourPurchaseTokenReadRepository)
    {
        _tourPurchaseTokenReadRepository = tourPurchaseTokenReadRepository;
    }

    public Task<bool> HasPurchasedAsync(Guid touristId, Guid tourId)
    {
        return _tourPurchaseTokenReadRepository.ExistsAsync(touristId, tourId);
    }

    public Task<List<PurchasedTourDto>> GetMyPurchasesAsync(Guid touristId)
    {
        return _tourPurchaseTokenReadRepository.GetPurchasedToursAsync(touristId);
    }
}
