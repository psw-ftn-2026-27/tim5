namespace Payment.Application.CouponCreating;

public sealed record CreateCouponDto(decimal DiscountPercentage, DateOnly ExpiresAt, Guid? ProductId);
