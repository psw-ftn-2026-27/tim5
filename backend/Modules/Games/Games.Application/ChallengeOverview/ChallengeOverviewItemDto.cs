using Games.Domain.SocialChallenges;

namespace Games.Application.ChallengeOverview;

public sealed record ChallengeOverviewItemDto(
    Guid Id,
    string Name,
    ChallengeKind Kind,
    int Xp,
    ChallengeStatus Status);
