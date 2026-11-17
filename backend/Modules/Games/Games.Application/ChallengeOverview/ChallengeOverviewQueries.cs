using Games.Application.LocationChallenges;
using Games.Application.Quizzes;
using Games.Application.SocialChallenges;

namespace Games.Application.ChallengeOverview;

public sealed class ChallengeOverviewQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly ISocialChallengeReadRepository _challengeReadRepository;
    private readonly ILocationChallengeReadRepository _locationChallengeReadRepository;

    public ChallengeOverviewQueries(
        IQuizReadRepository quizReadRepository,
        ISocialChallengeReadRepository challengeReadRepository,
        ILocationChallengeReadRepository locationChallengeReadRepository)
    {
        _quizReadRepository = quizReadRepository;
        _challengeReadRepository = challengeReadRepository;
        _locationChallengeReadRepository = locationChallengeReadRepository;
    }

    public async Task<List<ChallengeOverviewItemDto>> GetAllAsync(ChallengeOverviewFilterDto filter)
    {
        var quizzes = await _quizReadRepository.GetAllAsync(filter.Status);
        var socialChallenges = await _challengeReadRepository.GetAllAsync(filter.Status);
        var locationChallenges = await _locationChallengeReadRepository.GetAllAsync(filter.Status);

        var quizItems = quizzes
            .Select(quiz => new ChallengeOverviewItemDto(quiz.Id, quiz.Name, ChallengeKind.Quiz, quiz.Xp, quiz.Status));
        var socialChallengeItems = socialChallenges
            .Select(challenge => new ChallengeOverviewItemDto(challenge.Id, challenge.Name, ChallengeKind.SocialChallenge, challenge.Xp, challenge.Status));
        var locationChallengeItems = locationChallenges
            .Select(challenge => new ChallengeOverviewItemDto(challenge.Id, challenge.Name, ChallengeKind.LocationChallenge, challenge.Xp, challenge.Status));

        return [.. quizItems, .. socialChallengeItems, .. locationChallengeItems];
    }
}
