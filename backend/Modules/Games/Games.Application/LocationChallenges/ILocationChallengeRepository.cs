using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeRepository
{
    Task<LocationChallenge?> GetByIdAsync(Guid id);
    void Add(LocationChallenge challenge);
}
