using Payment.Application.CouponCreating;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<CouponDto?> GetByIdForOwnerAsync(Guid id, Guid ownerId);
}
