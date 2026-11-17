using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeInputDto(
    string Name,
    int Xp,
    LocationDto StartLocation);
