using System.Security.Cryptography;
using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;

namespace Payment.Application.CouponCreating;

public sealed class CouponCreatingService
{
    private const string CodeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CouponCreatingService(
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatedCouponDto> CreateCouponAsync(Guid ownerId, CreateCouponDto dto)
    {
        if (dto.ProductId is Guid productId)
        {
            var product = await _productRepository.GetByEntityAsync(productId);
            if (product is null)
            {
                throw new NotFoundException("The product does not exist.");
            }

            product.EnsureOwnedBy(ownerId);
        }

        var coupon = new Coupon(
            RandomNumberGenerator.GetString(CodeCharacters, 8),
            ownerId,
            dto.ProductId,
            dto.DiscountPercentage,
            dto.ExpiresAt,
            DateTime.UtcNow);

        _couponRepository.Add(coupon);
        await _unitOfWork.SaveChangesAsync();

        return new CreatedCouponDto(coupon.Id);
    }
}
