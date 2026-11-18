using Shared.Domain;

namespace Games.Domain.LocationChallengeActivations;

public sealed class LocationChallengeActivation : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid ChallengeId { get; private set; }
    public double Radius { get; private set; }
    public DateTime ActivatedAt { get; private set; }

    private LocationChallengeActivation() { }

    // Created only through LocationChallenge.Activate, which checks the challenge and computes the radius.
    // The radius is stored, so the user sees the same circle until they finish the challenge.
    internal LocationChallengeActivation(Guid userId, Guid challengeId, double radius)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        ChallengeId = challengeId;
        Radius = radius;
        ActivatedAt = DateTime.UtcNow;
    }
}
