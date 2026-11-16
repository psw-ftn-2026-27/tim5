using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid ImageId { get; private set; }
    public string? Secret { get; private set; }
    public bool IsPublic { get; private set; }
    public int Order { get; private set; }
    public Location Location { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        Location = null!;
    }

    // Created only through Tour, which decides the order and guards when key points may change.
    internal KeyPoint(string name, string description, Guid imageId, string? secret, bool isPublic, int order, Location location)
        : base(Guid.NewGuid())
    {
        Name = null!;
        Description = null!;
        Location = null!;
        Change(name, description, imageId, secret, isPublic, location);
        Order = order;
    }

    internal void Change(string name, string description, Guid imageId, string? secret, bool isPublic, Location location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (imageId == Guid.Empty)
        {
            throw new DomainException("A key point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A key point requires a location.");
        }

        Name = name;
        Description = description;
        ImageId = imageId;
        Secret = string.IsNullOrWhiteSpace(secret) ? null : secret;
        IsPublic = isPublic;
        Location = location;
    }

    internal void ChangeOrder(int order)
    {
        Order = order;
    }
}
