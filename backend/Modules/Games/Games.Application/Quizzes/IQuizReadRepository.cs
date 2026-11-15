using Games.Application.QuizAuthoring;
using Games.Domain.SocialChallenges;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<QuizSummaryDto>> GetAllAsync(ChallengeStatus? status);
    Task<QuizDto?> GetByIdAsync(Guid id);
}
