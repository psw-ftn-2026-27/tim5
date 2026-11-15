using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizAuthoringQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<List<QuizSummaryDto>> GetAllAsync()
    {
        return _quizReadRepository.GetAllAsync();
    }

    public async Task<QuizDto> GetByIdAsync(Guid quizId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException("The quiz no longer exists.");
        }

        return quiz;
    }
}
