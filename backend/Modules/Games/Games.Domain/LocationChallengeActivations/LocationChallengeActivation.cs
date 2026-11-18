using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallengeActivations;

public sealed class LocationChallengeActivation : AggregateRoot
{
    // There is no method that removes an ID, so a discovered point stays discovered.
    private readonly List<Guid> _discoveredPointIds = [];

    public Guid UserId { get; private set; }
    public Guid ChallengeId { get; private set; }
    public double Radius { get; private set; }
    public DateTime ActivatedAt { get; private set; }
    public IReadOnlyList<Guid> DiscoveredPointIds => _discoveredPointIds;
    public Completion? Completion { get; private set; }

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

    // Called by LocationChecker, which decides from the challenge which point is discovered.
    // The XP is recorded only here, once, when the last point is discovered.
    public void Discover(Guid pointId, int pointCount, int xp, DateTime now)
    {
        if (Completion is not null)
        {
            throw new DomainException("You have already completed this location challenge.");
        }

        _discoveredPointIds.Add(pointId);
        if (_discoveredPointIds.Count == pointCount)
        {
            Completion = new Completion(now, xp);
        }
    }
}
