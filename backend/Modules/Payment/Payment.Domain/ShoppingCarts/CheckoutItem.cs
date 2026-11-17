namespace Payment.Domain.ShoppingCarts;

public sealed record CheckoutItem(Guid EntityId, string Name, int Price, Guid OwnerId);
