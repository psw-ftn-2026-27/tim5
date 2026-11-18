using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeBrowsing;
using Games.Domain.SocialChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<List<LocationChallengeSummaryDto>> GetAllAsync(ChallengeStatus? status);
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
    Task<List<PublishedLocationChallengeDto>> GetPublishedAsync(Guid userId);
}
