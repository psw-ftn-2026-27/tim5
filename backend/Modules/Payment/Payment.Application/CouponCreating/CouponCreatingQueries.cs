using Payment.Application.Coupons;
using Shared.Domain.Exceptions;

namespace Payment.Application.CouponCreating;

public sealed class CouponCreatingQueries
{
    private readonly ICouponReadRepository _couponReadRepository;

    public CouponCreatingQueries(ICouponReadRepository couponReadRepository)
    {
        _couponReadRepository = couponReadRepository;
    }

    public async Task<CouponDto> GetCouponAsync(Guid id, Guid ownerId)
    {
        return await _couponReadRepository.GetByIdForOwnerAsync(id, ownerId)
            ?? throw new NotFoundException("The coupon does not exist.");
    }
}
