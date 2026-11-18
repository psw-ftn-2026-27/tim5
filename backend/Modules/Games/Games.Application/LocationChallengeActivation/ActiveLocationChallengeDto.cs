using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeActivation;

// Carries only the circle and the photos: the hidden points' locations must not reach the user.
public sealed record ActiveLocationChallengeDto(
    Guid ChallengeId,
    string Name,
    LocationDto Center,
    double RadiusInMeters,
    List<string> PhotoUrls);
