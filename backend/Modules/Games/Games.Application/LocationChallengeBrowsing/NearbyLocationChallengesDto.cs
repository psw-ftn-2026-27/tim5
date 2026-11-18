using Games.Application.Quizzes;

namespace Games.Application.LocationChallengeBrowsing;

// Position is null when the user has not set one yet; the list is then empty, because no distance can be computed.
public sealed record NearbyLocationChallengesDto(
    LocationDto? Position,
    List<NearbyLocationChallengeDto> Challenges);
