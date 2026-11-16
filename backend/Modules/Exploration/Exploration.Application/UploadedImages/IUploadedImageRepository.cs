using Shared.Domain.Images;

namespace Exploration.Application.UploadedImages;

public interface IUploadedImageRepository
{
    Task<UploadedImage?> GetByIdAsync(Guid id);
    Task<int> CountUnlinkedByOwnerAsync(Guid ownerId);
    void Add(UploadedImage image);
}
