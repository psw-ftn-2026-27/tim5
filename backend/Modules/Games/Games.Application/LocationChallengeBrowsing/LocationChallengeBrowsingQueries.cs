using Exploration.Contracts;
using Games.Application.LocationChallenges;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;

namespace Games.Application.LocationChallengeBrowsing;

public sealed class LocationChallengeBrowsingQueries
{
    private readonly ILocationChallengeReadRepository _challengeReadRepository;
    private readonly IExplorationApi _explorationApi;

    public LocationChallengeBrowsingQueries(ILocationChallengeReadRepository challengeReadRepository, IExplorationApi explorationApi)
    {
        _challengeReadRepository = challengeReadRepository;
        _explorationApi = explorationApi;
    }

    public async Task<NearbyLocationChallengesDto> GetNearbyAsync(Guid userId)
    {
        var position = await _explorationApi.GetCurrentPositionAsync(userId);
        if (position is null)
        {
            return new NearbyLocationChallengesDto(null, []);
        }

        var userLocation = new Location(position.Latitude, position.Longitude);
        var challenges = await _challengeReadRepository.GetPublishedAsync(userId);

        var nearby = challenges
            .Select(challenge => ToNearbyChallenge(challenge, userLocation))
            .OrderBy(challenge => challenge.DistanceInMeters)
            .ToList();
        return new NearbyLocationChallengesDto(new LocationDto(position.Latitude, position.Longitude), nearby);
    }

    // The distance comes from the domain's Location, so the read side does not repeat the formula in SQL.
    private static NearbyLocationChallengeDto ToNearbyChallenge(PublishedLocationChallengeDto challenge, Location userLocation)
    {
        var startLocation = new Location(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude);

        return new NearbyLocationChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.Xp,
            challenge.HiddenPointCount,
            challenge.StartLocation,
            userLocation.DistanceTo(startLocation),
            challenge.IsActivated);
    }
}
