namespace Payment.Domain.Wallets;

public sealed record PurchaseItem(Guid EntityId, string Name, Amount Price);
