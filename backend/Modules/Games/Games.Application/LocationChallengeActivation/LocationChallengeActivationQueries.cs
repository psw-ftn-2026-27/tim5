using Games.Application.LocationChallengeActivations;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeActivation;

public sealed class LocationChallengeActivationQueries
{
    private readonly ILocationChallengeActivationReadRepository _activationReadRepository;

    public LocationChallengeActivationQueries(ILocationChallengeActivationReadRepository activationReadRepository)
    {
        _activationReadRepository = activationReadRepository;
    }

    public async Task<ActiveLocationChallengeDto> GetActiveAsync(Guid userId, Guid challengeId)
    {
        var activeChallenge = await _activationReadRepository.GetActiveAsync(challengeId, userId);
        if (activeChallenge is null)
        {
            throw new NotFoundException("You have not activated this location challenge.");
        }

        return activeChallenge;
    }
}
