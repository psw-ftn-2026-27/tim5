using Shared.Domain.Images;

namespace Games.Application.UploadedImages;

public interface IUploadedImageRepository
{
    Task<UploadedImage?> GetByIdAsync(Guid id);
    Task<int> CountUnlinkedByOwnerAsync(Guid ownerId);
    void Add(UploadedImage image);
}
