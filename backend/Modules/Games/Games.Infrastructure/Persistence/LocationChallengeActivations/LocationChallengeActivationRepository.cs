using Games.Application.LocationChallengeActivations;
using Games.Domain.LocationChallengeActivations;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeActivations;

internal sealed class LocationChallengeActivationRepository : ILocationChallengeActivationRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeActivationRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeActivation?> GetByChallengeAndUserAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.LocationChallengeActivations
            .FirstOrDefaultAsync(activation => activation.ChallengeId == challengeId && activation.UserId == userId);
    }

    public void Add(LocationChallengeActivation activation)
    {
        _dbContext.LocationChallengeActivations.Add(activation);
    }
}
