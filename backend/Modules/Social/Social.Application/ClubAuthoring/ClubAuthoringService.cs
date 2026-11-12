using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Social.Application.Clubs;
using Social.Application.UploadedImages;
using Social.Domain.Clubs;

namespace Social.Application.ClubAuthoring;

public sealed class ClubAuthoringService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUploadedImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClubNameUniqueness _nameUniqueness;

    public ClubAuthoringService(
        IClubRepository clubRepository,
        IUploadedImageRepository imageRepository,
        IUnitOfWork unitOfWork,
        ClubNameUniqueness nameUniqueness)
    {
        _clubRepository = clubRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _nameUniqueness = nameUniqueness;
    }

    public async Task<Guid> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var name = new ClubName(dto.Name);
        var clubWithName = await _clubRepository.GetByNameAsync(name);
        _nameUniqueness.CheckForNewClub(name, clubWithName);
        var images = await GetImagesAsync(dto.ImageIds);

        var club = new Club(ownerId, name, dto.Description, dto.ImageIds);
        foreach (var image in images)
        {
            image.LinkTo(ownerId);
        }

        _clubRepository.Add(club);
        await _unitOfWork.SaveChangesAsync();

        return club.Id;
    }

    public async Task UpdateAsync(Guid clubId, Guid userId, UpdateClubDto dto)
    {
        var club = await GetClubAsync(clubId);
        var name = new ClubName(dto.Name);
        var clubWithName = await _clubRepository.GetByNameAsync(name);
        _nameUniqueness.CheckForRename(club, name, clubWithName);
        // Images the club already shows are linked; only the newly added ones still need linking.
        var addedImages = await GetImagesAsync(dto.ImageIds.Except(club.ImageIds));

        club.UpdateDetails(userId, name, dto.Description, dto.ImageIds);
        foreach (var image in addedImages)
        {
            image.LinkTo(userId);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.Delete(userId);
        _clubRepository.Remove(club);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }

    private async Task<List<UploadedImage>> GetImagesAsync(IEnumerable<Guid> imageIds)
    {
        var images = new List<UploadedImage>();
        foreach (var imageId in imageIds)
        {
            var image = await _imageRepository.GetByIdAsync(imageId);
            if (image is null)
            {
                throw new NotFoundException($"Image {imageId} does not exist.");
            }
            images.Add(image);
        }
        return images;
    }
}
