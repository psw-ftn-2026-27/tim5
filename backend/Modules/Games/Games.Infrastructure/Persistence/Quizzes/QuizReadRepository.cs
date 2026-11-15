using Games.Application.QuizAuthoring;
using Games.Application.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<QuizSummaryDto>> GetAllAsync()
    {
        return _dbContext.Quizzes
            .Select(quiz => new QuizSummaryDto(
                quiz.Id,
                quiz.Name,
                quiz.Xp,
                quiz.Questions.Count,
                quiz.Status))
            .ToListAsync();
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes
            .Where(quiz => quiz.Id == id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Xp,
                new LocationDto(quiz.Location.Latitude, quiz.Location.Longitude),
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Text,
                        question.Answers
                            .Select(answer => new AnswerDto(answer.Text, answer.IsCorrect))
                            .ToList()))
                    .ToList(),
                quiz.Status))
            .FirstOrDefaultAsync();
    }
}
