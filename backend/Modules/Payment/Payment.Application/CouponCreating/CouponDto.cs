namespace Payment.Application.CouponCreating;

public sealed record CouponDto(Guid Id, string Code, decimal DiscountPercentage, DateOnly ExpiresAt, Guid? ProductId);
