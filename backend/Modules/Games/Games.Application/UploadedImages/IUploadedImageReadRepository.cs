namespace Games.Application.UploadedImages;

public interface IUploadedImageReadRepository
{
    Task<ImageFileDto?> GetFileAsync(Guid id);
}
