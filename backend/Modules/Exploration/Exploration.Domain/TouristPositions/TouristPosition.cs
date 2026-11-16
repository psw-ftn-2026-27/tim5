using Exploration.Domain.Tours;
using Shared.Domain;

namespace Exploration.Domain.TouristPositions;

public sealed class TouristPosition : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Location Location { get; private set; }
    public DateTime MarkedAt { get; private set; }

    private TouristPosition()
    {
        Location = null!;
    }

    public TouristPosition(Guid userId, Location location, DateTime markedAt)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        Location = location;
        MarkedAt = markedAt;
    }

    public void MarkAt(Location location, DateTime markedAt)
    {
        Location = location;
        MarkedAt = markedAt;
    }
}
