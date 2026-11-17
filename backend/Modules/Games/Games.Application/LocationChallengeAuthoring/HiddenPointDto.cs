using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record HiddenPointDto(
    Guid Id,
    Guid ImageId,
    string ImageUrl,
    LocationDto Location);
