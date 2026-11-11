using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public static class UploadQuota
{
    public const int MaxUnlinkedImages = 20;

    public static UploadedImage CreateImage(Guid ownerId, int unlinkedImageCount, ImageFile file)
    {
        if (unlinkedImageCount >= MaxUnlinkedImages)
        {
            throw new DomainException("A user cannot hold more than 20 unlinked images.");
        }

        return new UploadedImage(ownerId, file);
    }
}
