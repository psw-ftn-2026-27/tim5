using Payment.Domain.Products;
using Shared.Domain;

namespace Payment.Domain.ShoppingCarts;

public sealed class OrderItem : Entity
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = null!;
    public Price Price { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(Guid entityId, string name, Price price) : base(Guid.NewGuid())
    {
        EntityId = entityId;
        Name = name;
        Price = price;
    }
}
