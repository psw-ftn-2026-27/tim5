namespace Games.Application.LocationChallengeActivations;

// The aggregate's name is qualified because, inside Games.Application, the simple name
// LocationChallengeActivation means the use-case group's namespace.
public interface ILocationChallengeActivationRepository
{
    Task<Domain.LocationChallengeActivations.LocationChallengeActivation?> GetByChallengeAndUserAsync(Guid challengeId, Guid userId);
    void Add(Domain.LocationChallengeActivations.LocationChallengeActivation activation);
}
