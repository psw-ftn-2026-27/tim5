using Games.Application.UploadedImages;
using Shared.Domain.Exceptions;

namespace Games.Application.ImageViewing;

public sealed class ImageViewingQueries
{
    private readonly IUploadedImageReadRepository _imageReadRepository;

    public ImageViewingQueries(IUploadedImageReadRepository imageReadRepository)
    {
        _imageReadRepository = imageReadRepository;
    }

    public async Task<ImageFileDto> GetImageFileAsync(Guid imageId)
    {
        var file = await _imageReadRepository.GetFileAsync(imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return file;
    }
}
