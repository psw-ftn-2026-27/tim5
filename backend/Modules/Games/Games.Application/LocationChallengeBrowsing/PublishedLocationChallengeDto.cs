using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeBrowsing;

public sealed record PublishedLocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    int HiddenPointCount,
    LocationDto StartLocation,
    bool IsActivated);
