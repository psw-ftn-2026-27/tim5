using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    public string Code { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public Guid? ProductId { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public DateOnly ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Coupon() { }

    public Coupon(string code, Guid ownerId, Guid? productId, decimal discountPercentage, DateOnly expiresAt, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (code.Length != 8)
        {
            throw new DomainException("A coupon code must contain exactly eight characters.");
        }

        if (discountPercentage < 1 || discountPercentage > 100 || decimal.Truncate(discountPercentage) != discountPercentage)
        {
            throw new DomainException("A coupon discount must be a whole percentage from 1 to 100.");
        }

        if (expiresAt < DateOnly.FromDateTime(createdAt))
        {
            throw new DomainException("A coupon cannot expire before its creation date.");
        }

        Code = code;
        OwnerId = ownerId;
        ProductId = productId;
        DiscountPercentage = discountPercentage;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }
}
