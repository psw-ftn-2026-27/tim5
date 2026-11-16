using Shared.Domain;

namespace Payment.Domain.Products;

public sealed class Product : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid OwnerId { get; private set; }
    public Price Price { get; private set; } = null!;

    private Product() { }

    public Product(Guid entityId, string name, Guid ownerId, Price price) : base(Guid.NewGuid())
    {
        EntityId = entityId;
        Name = name;
        OwnerId = ownerId;
        Price = price;
    }

    public void UpdateListing(string name, Price price)
    {
        Name = name;
        Price = price;
    }
}
