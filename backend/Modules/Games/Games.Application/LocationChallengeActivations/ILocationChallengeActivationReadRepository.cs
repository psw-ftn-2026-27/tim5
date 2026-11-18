using Games.Application.LocationChallengeActivation;

namespace Games.Application.LocationChallengeActivations;

public interface ILocationChallengeActivationReadRepository
{
    Task<ActiveLocationChallengeDto?> GetActiveAsync(Guid challengeId, Guid userId);
}
