using Games.Application.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringQueries
{
    private readonly ILocationChallengeReadRepository _challengeReadRepository;

    public LocationChallengeAuthoringQueries(ILocationChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<List<LocationChallengeSummaryDto>> GetAllAsync()
    {
        return _challengeReadRepository.GetAllAsync(null);
    }

    public async Task<LocationChallengeDto> GetByIdAsync(Guid challengeId)
    {
        var challenge = await _challengeReadRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException("The location challenge no longer exists.");
        }

        return challenge;
    }
}
