using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizLifecycle;

public sealed class QuizLifecycleService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizLifecycleService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DeleteAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException("The quiz no longer exists.");
        }

        quiz.Delete();
        _quizRepository.Remove(quiz);
        await _unitOfWork.SaveChangesAsync();
    }
}
