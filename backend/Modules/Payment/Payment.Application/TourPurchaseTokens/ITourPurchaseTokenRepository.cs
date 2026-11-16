using Payment.Domain.TourPurchaseTokens;

namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenRepository
{
    Task<TourPurchaseToken?> GetByTouristAndEntityAsync(Guid touristId, Guid entityId);
    void Add(TourPurchaseToken token);
}
