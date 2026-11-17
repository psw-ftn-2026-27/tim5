using Payment.Application.Shopping;

namespace Payment.Application.Purchases;

public interface IPurchaseReadRepository
{
    Task<PurchaseDto?> GetByIdForTouristAsync(Guid id, Guid touristId);
}
