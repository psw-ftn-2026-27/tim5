using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponCreating;
using Payment.Application.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CouponDto?> GetByIdForOwnerAsync(Guid id, Guid ownerId)
    {
        return _dbContext.Coupons
            .Where(coupon => coupon.Id == id && coupon.OwnerId == ownerId)
            .Select(coupon => new CouponDto(
                coupon.Id,
                coupon.Code,
                coupon.DiscountPercentage,
                coupon.ExpiresAt,
                coupon.ProductId))
            .FirstOrDefaultAsync();
    }
}
