using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public ClubName Name { get; private set; }
    public string Description { get; private set; }
    public List<Guid> ImageIds { get; private set; }

    private Club()
    {
        Name = null!;
        Description = null!;
        ImageIds = null!;
    }

    public Club(Guid ownerId, ClubName name, string description, List<Guid> imageIds)
        : base(Guid.NewGuid())
    {
        EnsureValidDetails(name, description, imageIds);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        ImageIds = imageIds;
    }

    public void UpdateDetails(Guid userId, ClubName name, string description, List<Guid> imageIds)
    {
        EnsureOwner(userId);
        EnsureValidDetails(name, description, imageIds);

        Name = name;
        Description = description;
        ImageIds = imageIds;
    }

    public void Delete(Guid userId)
    {
        EnsureOwner(userId);
    }

    private void EnsureOwner(Guid userId)
    {
        if (userId != OwnerId)
        {
            throw new DomainException("Only the owner can change or delete a club.");
        }
    }

    private static void EnsureValidDetails(ClubName name, string description, List<Guid> imageIds)
    {
        if (name is null)
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (imageIds is null || imageIds.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }
    }
}
