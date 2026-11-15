using Games.Domain.SocialChallenges;

namespace Games.Application.QuizAuthoring;

public sealed record QuizSummaryDto(
    Guid Id,
    string Name,
    int Xp,
    int QuestionCount,
    ChallengeStatus Status);
