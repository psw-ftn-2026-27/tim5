using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Games.Application.Quizzes;
using Games.Application.UploadedImages;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<LocationChallengeSummaryDto>> GetAllAsync(ChallengeStatus? status)
    {
        var challenges = status is null
            ? _dbContext.LocationChallenges
            : _dbContext.LocationChallenges.Where(challenge => challenge.Status == status);

        return challenges
            .Select(challenge => new LocationChallengeSummaryDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                challenge.HiddenPoints.Count,
                challenge.Status))
            .ToListAsync();
    }

    public Task<LocationChallengeDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges
            .Where(challenge => challenge.Id == id)
            .Select(challenge => new LocationChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                challenge.Status,
                new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                challenge.HiddenPoints
                    .Select(point => new HiddenPointDto(
                        point.Id,
                        point.ImageId,
                        ImageAddress.For(point.ImageId),
                        new LocationDto(point.Location.Latitude, point.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
