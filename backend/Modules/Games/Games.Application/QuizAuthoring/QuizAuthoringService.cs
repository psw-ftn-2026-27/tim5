using AutoMapper;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuizAuthoringService(IQuizRepository quizRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<QuizDto> CreateAsync(QuizInputDto dto)
    {
        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.Xp, location, CreateQuestions(dto.Questions));

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<QuizDto>(quiz);
    }

    public async Task UpdateAsync(Guid quizId, QuizInputDto dto)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException("The quiz no longer exists.");
        }

        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        quiz.Update(dto.Name, dto.Instructions, dto.Xp, location, CreateQuestions(dto.Questions));
        await _unitOfWork.SaveChangesAsync();
    }

    private static List<Question> CreateQuestions(List<QuestionDto> questions)
    {
        return questions
            .Select(question => new Question(
                question.Text,
                question.Answers.Select(answer => new Answer(answer.Text, answer.IsCorrect)).ToList()))
            .ToList();
    }
}
