using Shared.Domain.Images;

namespace Social.Application.UploadedImages;

public interface IUploadedImageRepository
{
    Task<UploadedImage?> GetByIdAsync(Guid id);
    Task<int> CountUnlinkedByOwnerAsync(Guid ownerId);
    void Add(UploadedImage image);
    void Remove(UploadedImage image);
}
