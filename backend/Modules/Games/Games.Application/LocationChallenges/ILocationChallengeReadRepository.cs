using Games.Application.LocationChallengeAuthoring;
using Games.Domain.SocialChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<List<LocationChallengeSummaryDto>> GetAllAsync(ChallengeStatus? status);
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
}
