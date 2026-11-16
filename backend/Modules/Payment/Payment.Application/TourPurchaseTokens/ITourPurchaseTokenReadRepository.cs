namespace Payment.Application.TourPurchaseTokens;

public interface ITourPurchaseTokenReadRepository
{
    Task<bool> ExistsAsync(Guid touristId, Guid entityId);
}
