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

    // Projects only the photo addresses of the hidden points, never their locations.
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
                        .Select(point => ImageAddress.For(point.ImageId))
                        .ToList()))
            .FirstOrDefaultAsync();
    }
}
