using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeActivation;

// Carries the circle and the photos. A hidden point's location reaches the user only after they discover it.
public sealed record ActiveLocationChallengeDto(
    Guid ChallengeId,
    string Name,
    LocationDto Center,
    double RadiusInMeters,
    List<HiddenPointPhotoDto> Photos,
    int DiscoveredPointCount,
    CompletionDto? Completion);
