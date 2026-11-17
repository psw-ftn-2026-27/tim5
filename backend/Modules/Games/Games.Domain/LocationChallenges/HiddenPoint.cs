using Games.Domain.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class HiddenPoint : Entity
{
    public Guid ImageId { get; private set; }
    public Location Location { get; private set; }

    private HiddenPoint()
    {
        Location = null!;
    }

    // Created only through LocationChallenge, which guards when hidden points may change.
    internal HiddenPoint(Guid imageId, Location location) : base(Guid.NewGuid())
    {
        Location = null!;
        Change(imageId, location);
    }

    internal void Change(Guid imageId, Location location)
    {
        if (imageId == Guid.Empty)
        {
            throw new DomainException("A hidden point requires a photo.");
        }
        if (location is null)
        {
            throw new DomainException("A hidden point requires a location.");
        }

        ImageId = imageId;
        Location = location;
    }
}
