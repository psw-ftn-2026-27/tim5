using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeActivations;

public static class LocationChecker
{
    private const double DiscoveryDistanceInMeters = 5;

    // Any undiscovered point can be discovered, in any order.
    public static void Check(LocationChallenge challenge, LocationChallengeActivation activation, Location position, DateTime now)
    {
        var discoveredPoint = challenge.HiddenPoints.FirstOrDefault(point =>
            !activation.DiscoveredPointIds.Contains(point.Id)
            && position.DistanceTo(point.Location) <= DiscoveryDistanceInMeters);
        if (discoveredPoint is null)
        {
            throw new DomainException("You are not at a hidden location of this challenge.");
        }

        activation.Discover(discoveredPoint.Id, challenge.HiddenPoints.Count, challenge.Xp, now);
    }
}
