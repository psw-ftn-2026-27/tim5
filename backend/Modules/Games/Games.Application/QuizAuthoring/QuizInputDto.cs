using Games.Application.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed record QuizInputDto(
    string Name,
    string Instructions,
    int Xp,
    LocationDto Location,
    List<QuestionDto> Questions);
