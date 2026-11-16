namespace Payment.Application.Purchasing;

public sealed record PurchasedTourDto(Guid EntityId, string Name, DateTime PurchasedAt);
