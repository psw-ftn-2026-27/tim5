using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed class Club : AggregateRoot
{
    private readonly List<ClubImage> _images = [];

    public Guid OwnerId { get; private set; }
    public ClubName Name { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyList<ClubImage> Images => _images;

    private Club()
    {
        Name = null!;
        Description = null!;
    }

    public Club(Guid ownerId, ClubName name, string description, List<ClubImage> images)
        : base(Guid.NewGuid())
    {
        EnsureValidDetails(name, description, images);

        OwnerId = ownerId;
        Name = name;
        Description = description;
        _images.AddRange(images);
    }

    public void UpdateDetails(Guid userId, ClubName name, string description, List<ClubImage> images)
    {
        EnsureOwner(userId);
        EnsureValidDetails(name, description, images);

        Name = name;
        Description = description;
        _images.Clear();
        _images.AddRange(images);
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

    private static void EnsureValidDetails(ClubName name, string description, List<ClubImage> images)
    {
        if (name is null)
        {
            throw new DomainException("A club requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A club requires a description.");
        }
        if (images is null || images.Count == 0)
        {
            throw new DomainException("A club requires at least one image.");
        }
    }
}
