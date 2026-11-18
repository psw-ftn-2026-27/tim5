using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeActivations;

public static class LocationChallengeActivator
{
    public static LocationChallengeActivation Activate(
        LocationChallenge challenge,
        LocationChallengeActivation? existingActivation,
        Guid userId,
        Location userPosition,
        double extraMeters)
    {
        if (existingActivation is not null)
        {
            throw new DomainException("You have already activated this location challenge.");
        }

        return challenge.Activate(userId, userPosition, extraMeters);
    }
}
