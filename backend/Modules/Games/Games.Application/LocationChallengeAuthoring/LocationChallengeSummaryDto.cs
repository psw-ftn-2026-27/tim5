using Games.Domain.SocialChallenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeSummaryDto(
    Guid Id,
    string Name,
    int Xp,
    int HiddenPointCount,
    ChallengeStatus Status);
