using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubAuthoring;

public sealed class ClubAuthoringService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClubNameUniqueness _nameUniqueness;

    public ClubAuthoringService(IClubRepository clubRepository, IUnitOfWork unitOfWork, ClubNameUniqueness nameUniqueness)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _nameUniqueness = nameUniqueness;
    }

    public async Task<Guid> CreateAsync(Guid ownerId, CreateClubDto dto)
    {
        var name = new ClubName(dto.Name);
        var clubWithName = await _clubRepository.GetByNameAsync(name);
        _nameUniqueness.CheckForNewClub(name, clubWithName);

        var club = new Club(ownerId, name, dto.Description, ToImages(dto.Images));

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

        club.UpdateDetails(userId, name, dto.Description, ToImages(dto.Images));
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

    private static List<ClubImage> ToImages(List<string> urls)
    {
        return urls.Select(url => new ClubImage(url)).ToList();
    }
}
