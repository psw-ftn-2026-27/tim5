using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeBrowsing;

public sealed record NearbyLocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    int HiddenPointCount,
    LocationDto StartLocation,
    double DistanceInMeters,
    bool IsActivated);
