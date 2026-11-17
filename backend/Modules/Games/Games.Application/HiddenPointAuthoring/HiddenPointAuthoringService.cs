using Games.Application.LocationChallenges;
using Games.Application.Quizzes;
using Games.Application.UploadedImages;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;

namespace Games.Application.HiddenPointAuthoring;

public sealed class HiddenPointAuthoringService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUploadedImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HiddenPointAuthoringService(
        ILocationChallengeRepository challengeRepository,
        IUploadedImageRepository imageRepository,
        IUnitOfWork unitOfWork)
    {
        _challengeRepository = challengeRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UploadedImageDto> UploadImageAsync(Guid administratorId, byte[] content)
    {
        var file = new ImageFile(content);
        var unlinkedImageCount = await _imageRepository.CountUnlinkedByOwnerAsync(administratorId);
        var image = UploadQuota.CreateImage(administratorId, unlinkedImageCount, file);

        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        return new UploadedImageDto(image.Id, ImageAddress.For(image.Id));
    }

    public async Task AddHiddenPointAsync(Guid challengeId, Guid administratorId, HiddenPointInputDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var image = await GetImageAsync(dto.ImageId);

        challenge.AddHiddenPoint(dto.ImageId, ToLocation(dto.Location));
        image.LinkTo(administratorId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeHiddenPointAsync(Guid challengeId, Guid pointId, Guid administratorId, HiddenPointInputDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);
        // The photo the hidden point already shows is linked; only a newly chosen one still needs linking.
        var imageIsNew = challenge.HiddenPoints.Any(point => point.Id == pointId && point.ImageId != dto.ImageId);
        var newImage = imageIsNew ? await GetImageAsync(dto.ImageId) : null;

        challenge.ChangeHiddenPoint(pointId, dto.ImageId, ToLocation(dto.Location));
        newImage?.LinkTo(administratorId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveHiddenPointAsync(Guid challengeId, Guid pointId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.RemoveHiddenPoint(pointId);
        await _unitOfWork.SaveChangesAsync();
    }

    private static Location ToLocation(LocationDto dto) => new(dto.Latitude, dto.Longitude);

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException("The location challenge no longer exists.");
        }
        return challenge;
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
