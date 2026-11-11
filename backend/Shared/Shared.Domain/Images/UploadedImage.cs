using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed class UploadedImage : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public string StoredName { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public bool IsLinked { get; private set; }
    public ImageFile File { get; private set; }

    private UploadedImage()
    {
        StoredName = null!;
        File = null!;
    }

    // Created only through UploadQuota, which guards how many unlinked images an owner may hold.
    internal UploadedImage(Guid ownerId, ImageFile file) : base(Guid.NewGuid())
    {
        OwnerId = ownerId;
        StoredName = Id.ToString("N");
        UploadedAt = DateTime.UtcNow;
        IsLinked = false;
        File = file;
    }

    public void LinkTo(Guid requesterId)
    {
        if (OwnerId != requesterId)
        {
            throw new DomainException("Only an image uploaded by the same user can be linked.");
        }
        if (IsLinked)
        {
            throw new DomainException("The image is already linked.");
        }

        IsLinked = true;
    }

    public void Delete(Guid requesterId)
    {
        if (OwnerId != requesterId)
        {
            throw new DomainException("Only the user who uploaded an image can delete it.");
        }
        if (IsLinked)
        {
            throw new DomainException("A linked image cannot be deleted.");
        }
    }
}
