using Payment.Domain.Wallets;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public Amount Amount { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RedeemedAt { get; private set; }

    private GiftCard() { }

    public GiftCard(Guid senderId, Guid recipientId, Amount amount, string message, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (senderId == recipientId)
        {
            throw new DomainException("A gift card cannot be sent to its sender.");
        }

        if (amount.Value is < 10 or > 1000)
        {
            throw new DomainException("A gift card amount must be between 10 and 1000 EC.");
        }

        if (message.Length > 200)
        {
            throw new DomainException("A gift card message cannot exceed 200 characters.");
        }

        SenderId = senderId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.AddYears(3);
    }

    public GiftCardStatus GetStatus(DateTime now) => GetStatus(ExpiresAt, RedeemedAt, now);

    public static GiftCardStatus GetStatus(DateTime expiresAt, DateTime? redeemedAt, DateTime now)
    {
        if (redeemedAt is not null)
        {
            return GiftCardStatus.Redeemed;
        }

        return now >= expiresAt ? GiftCardStatus.Expired : GiftCardStatus.Valid;
    }

    public void Redeem(Guid recipientId, DateTime now)
    {
        if (recipientId != RecipientId)
        {
            throw new DomainException("Only the recipient can redeem this gift card.");
        }

        if (GetStatus(now) != GiftCardStatus.Valid)
        {
            throw new DomainException("An expired or redeemed gift card cannot be redeemed.");
        }

        RedeemedAt = now;
    }
}
