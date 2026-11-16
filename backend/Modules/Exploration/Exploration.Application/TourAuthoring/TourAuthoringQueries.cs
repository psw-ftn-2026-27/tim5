using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly IUploadedImageReadRepository _imageReadRepository;

    public TourAuthoringQueries(ITourReadRepository tourReadRepository, IUploadedImageReadRepository imageReadRepository)
    {
        _tourReadRepository = tourReadRepository;
        _imageReadRepository = imageReadRepository;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return _tourReadRepository.GetByAuthorAsync(authorId);
    }

    public async Task<TourWithKeyPointsDto> GetOwnedTourWithKeyPointsAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourReadRepository.GetOwnedWithKeyPointsAsync(tourId, authorId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return tour;
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
