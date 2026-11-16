using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class ShoppingCart : AggregateRoot
{
    public Guid TouristId { get; private set; }
    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items;
    public int Total => _items.Sum(item => item.Price.Value);

    private ShoppingCart() { }

    public ShoppingCart(Guid touristId) : base(Guid.NewGuid())
    {
        TouristId = touristId;
    }

    public void AddItem(Guid entityId, string name, Price price, Guid ownerId, bool isPurchased)
    {
        if (price.Value == 0)
        {
            throw new DomainException("A free tour cannot be added to the cart.");
        }

        if (_items.Any(item => item.EntityId == entityId))
        {
            throw new DomainException("The tour is already in the cart.");
        }

        if (isPurchased)
        {
            throw new DomainException("The tour has already been purchased.");
        }

        if (ownerId == TouristId)
        {
            throw new DomainException("A tourist cannot add their own tour to the cart.");
        }

        _items.Add(new OrderItem(entityId, name, price));
    }

    public IReadOnlyList<OrderItem> Checkout()
    {
        if (_items.Count == 0)
        {
            throw new DomainException("An empty cart cannot be paid.");
        }

        var items = _items.ToList();
        _items.Clear();
        return items;
    }

    public void RemoveItem(Guid entityId)
    {
        var item = _items.FirstOrDefault(item => item.EntityId == entityId)
            ?? throw new NotFoundException($"Tour {entityId} is not in the cart.");

        _items.Remove(item);
    }
}
