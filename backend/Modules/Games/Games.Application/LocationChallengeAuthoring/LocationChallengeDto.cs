using Games.Application.Quizzes;
using Games.Domain.SocialChallenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    ChallengeStatus Status,
    LocationDto StartLocation,
    List<HiddenPointDto> HiddenPoints);
