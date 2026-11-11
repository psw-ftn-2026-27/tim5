using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeAuthoring;

public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    string ExitMessage,
    int RequiredPeople,
    int RangeInMeters,
    int Xp,
    TimeWindowDto Window,
    ChallengeStatus Status);
