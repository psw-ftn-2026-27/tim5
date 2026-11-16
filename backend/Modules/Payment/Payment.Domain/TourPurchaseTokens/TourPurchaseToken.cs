using Shared.Domain;

namespace Payment.Domain.TourPurchaseTokens;

public sealed class TourPurchaseToken : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Guid EntityId { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    private TourPurchaseToken() { }

    public TourPurchaseToken(Guid touristId, Guid entityId, DateTime purchasedAt) : base(Guid.NewGuid())
    {
        TouristId = touristId;
        EntityId = entityId;
        PurchasedAt = purchasedAt;
    }
}
