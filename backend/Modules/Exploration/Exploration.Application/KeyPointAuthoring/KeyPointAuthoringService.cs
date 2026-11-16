using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUploadedImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public KeyPointAuthoringService(
        ITourRepository tourRepository,
        IUploadedImageRepository imageRepository,
        IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadedImageDto> UploadImageAsync(Guid authorId, byte[] content)
    {
        var file = new ImageFile(content);
        var unlinkedImageCount = await _imageRepository.CountUnlinkedByOwnerAsync(authorId);
        var image = UploadQuota.CreateImage(authorId, unlinkedImageCount, file);

        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        return new UploadedImageDto(image.Id, ImageAddress.For(image.Id));
    }

    public async Task AddKeyPointAsync(Guid tourId, Guid authorId, AddKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var image = await GetImageAsync(dto.ImageId);

        tour.AddKeyPoint(dto.Name, dto.Description, dto.ImageId, dto.Secret, dto.IsPublic, ToLocation(dto.Location));
        image.LinkTo(authorId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeKeyPointAsync(Guid tourId, Guid keyPointId, Guid authorId, ChangeKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        // The image the key point already shows is linked; only a newly chosen one still needs linking.
        var imageIsNew = tour.KeyPoints.Any(keyPoint => keyPoint.Id == keyPointId && keyPoint.ImageId != dto.ImageId);
        var newImage = imageIsNew ? await GetImageAsync(dto.ImageId) : null;

        tour.ChangeKeyPoint(keyPointId, dto.Name, dto.Description, dto.ImageId, dto.Secret, dto.IsPublic, ToLocation(dto.Location));
        newImage?.LinkTo(authorId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeyPointAsync(Guid tourId, Guid keyPointId, Guid authorId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.RemoveKeyPoint(keyPointId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RecordLengthAsync(Guid tourId, Guid authorId, TourLengthDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.RecordLength(dto.LengthInKm);
        await _unitOfWork.SaveChangesAsync();
    }

    private static Location ToLocation(LocationDto dto) => new(dto.Latitude, dto.Longitude);

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }

    private async Task<UploadedImage> GetImageAsync(Guid imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return image;
    }
}
