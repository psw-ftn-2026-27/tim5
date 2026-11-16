using Payment.Application.Purchasing;

namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenReadRepository
{
    Task<bool> ExistsAsync(Guid touristId, Guid entityId);
    Task<List<PurchasedTourDto>> GetPurchasedToursAsync(Guid touristId);
}
