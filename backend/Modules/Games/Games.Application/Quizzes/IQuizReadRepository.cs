using Games.Application.QuizAuthoring;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<QuizSummaryDto>> GetAllAsync();
    Task<QuizDto?> GetByIdAsync(Guid id);
}
