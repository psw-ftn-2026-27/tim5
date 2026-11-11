using Games.Application.SocialChallenges;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringQueries
{
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public SocialChallengeAuthoringQueries(ISocialChallengeReadRepository challengeReadRepository)
    {
        _challengeReadRepository = challengeReadRepository;
    }

    public Task<List<SocialChallengeDto>> GetAllAsync(SocialChallengeFilterDto filter)
    {
        return _challengeReadRepository.GetAllAsync(filter.Status);
    }
}
