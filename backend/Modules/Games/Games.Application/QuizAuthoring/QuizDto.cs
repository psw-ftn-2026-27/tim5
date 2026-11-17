using Games.Application.Quizzes;
using Games.Domain.SocialChallenges;

namespace Games.Application.QuizAuthoring;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string Instructions,
    int Xp,
    LocationDto Location,
    List<QuestionDto> Questions,
    ChallengeStatus Status);
