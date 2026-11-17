using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchases;
using Payment.Application.Shopping;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseReadRepository : IPurchaseReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PurchaseDto?> GetByIdForTouristAsync(Guid id, Guid touristId)
    {
        return _dbContext.Purchases
            .Where(purchase => purchase.Id == id && purchase.TouristId == touristId)
            .Select(purchase => new PurchaseDto(
                purchase.Id,
                purchase.Total,
                purchase.CouponId,
                purchase.CouponDiscount,
                purchase.GiftCardId,
                purchase.GiftCardDiscount,
                purchase.PurchasedAt))
            .FirstOrDefaultAsync();
    }
}
