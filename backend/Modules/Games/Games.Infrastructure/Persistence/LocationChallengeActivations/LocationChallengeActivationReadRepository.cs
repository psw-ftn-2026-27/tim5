using Games.Application.LocationChallengeActivation;
using Games.Application.LocationChallengeActivations;
using Games.Application.Quizzes;
using Games.Application.UploadedImages;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeActivations;

internal sealed class LocationChallengeActivationReadRepository : ILocationChallengeActivationReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeActivationReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Projects the photo addresses of the hidden points, and a point's location only once the user discovered it.
    public Task<ActiveLocationChallengeDto?> GetActiveAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.LocationChallengeActivations
            .Where(activation => activation.ChallengeId == challengeId && activation.UserId == userId)
            .Join(
                _dbContext.LocationChallenges,
                activation => activation.ChallengeId,
                challenge => challenge.Id,
                (activation, challenge) => new ActiveLocationChallengeDto(
                    challenge.Id,
                    challenge.Name,
                    new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                    activation.Radius,
                    challenge.HiddenPoints
                        .Select(point => new HiddenPointPhotoDto(
                            ImageAddress.For(point.ImageId),
                            activation.DiscoveredPointIds.Contains(point.Id),
                            activation.DiscoveredPointIds.Contains(point.Id)
                                ? new LocationDto(point.Location.Latitude, point.Location.Longitude)
                                : null))
                        .ToList(),
                    activation.DiscoveredPointIds.Count,
                    activation.Completion == null
                        ? null
                        : new CompletionDto(activation.Completion.CompletedAt, activation.Completion.Xp)))
            .FirstOrDefaultAsync();
    }
}
