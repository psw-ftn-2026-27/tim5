using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeActivation;

// Location is null until the point is discovered.
public sealed record HiddenPointPhotoDto(string PhotoUrl, bool IsDiscovered, LocationDto? Location);
