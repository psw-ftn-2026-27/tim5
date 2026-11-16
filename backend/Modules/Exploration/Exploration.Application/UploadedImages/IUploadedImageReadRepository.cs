namespace Exploration.Application.UploadedImages;

public interface IUploadedImageReadRepository
{
    Task<ImageFileDto?> GetFileAsync(Guid id);
}
