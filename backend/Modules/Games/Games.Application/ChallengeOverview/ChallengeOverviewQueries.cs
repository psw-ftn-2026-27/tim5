using Games.Application.Quizzes;
using Games.Application.SocialChallenges;

namespace Games.Application.ChallengeOverview;

public sealed class ChallengeOverviewQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly ISocialChallengeReadRepository _challengeReadRepository;

    public ChallengeOverviewQueries(
        IQuizReadRepository quizReadRepository,
        ISocialChallengeReadRepository challengeReadRepository)
    {
        _quizReadRepository = quizReadRepository;
        _challengeReadRepository = challengeReadRepository;
    }

    public async Task<List<ChallengeOverviewItemDto>> GetAllAsync(ChallengeOverviewFilterDto filter)
    {
        var quizzes = await _quizReadRepository.GetAllAsync(filter.Status);
        var socialChallenges = await _challengeReadRepository.GetAllAsync(filter.Status);

        var quizItems = quizzes
            .Select(quiz => new ChallengeOverviewItemDto(quiz.Id, quiz.Name, ChallengeKind.Quiz, quiz.Xp, quiz.Status));
        var socialChallengeItems = socialChallenges
            .Select(challenge => new ChallengeOverviewItemDto(challenge.Id, challenge.Name, ChallengeKind.SocialChallenge, challenge.Xp, challenge.Status));

        return [.. quizItems, .. socialChallengeItems];
    }
}
