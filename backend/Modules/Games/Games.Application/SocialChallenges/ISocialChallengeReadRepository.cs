using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<List<SocialChallengeDto>> GetAllAsync(ChallengeStatus? status);
}
