using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    private readonly List<HiddenPoint> _hiddenPoints = [];

    public string Name { get; private set; }
    public int Xp { get; private set; }
    public Location StartLocation { get; private set; }
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints;
    public ChallengeStatus Status { get; private set; }

    private LocationChallenge()
    {
        Name = null!;
        StartLocation = null!;
    }

    public LocationChallenge(string name, int xp, Location startLocation)
        : base(Guid.NewGuid())
    {
        Validate(name, xp, startLocation);

        Name = name;
        Xp = xp;
        StartLocation = startLocation;
        Status = ChallengeStatus.Draft;
    }

    public void Update(string name, int xp, Location startLocation)
    {
        if (!CanModify())
        {
            throw new DomainException("Only a draft location challenge can be changed.");
        }
        Validate(name, xp, startLocation);

        Name = name;
        Xp = xp;
        StartLocation = startLocation;
    }

    public void AddHiddenPoint(Guid imageId, Location location)
    {
        if (!CanModify())
        {
            throw new DomainException("Hidden points can be changed only while the location challenge is a draft.");
        }

        _hiddenPoints.Add(new HiddenPoint(imageId, location));
    }

    public void ChangeHiddenPoint(Guid pointId, Guid imageId, Location location)
    {
        if (!CanModify())
        {
            throw new DomainException("Hidden points can be changed only while the location challenge is a draft.");
        }

        GetHiddenPoint(pointId).Change(imageId, location);
    }

    public void RemoveHiddenPoint(Guid pointId)
    {
        if (!CanModify())
        {
            throw new DomainException("Hidden points can be changed only while the location challenge is a draft.");
        }

        _hiddenPoints.Remove(GetHiddenPoint(pointId));
    }

    private bool CanModify()
    {
        return Status == ChallengeStatus.Draft;
    }

    private HiddenPoint GetHiddenPoint(Guid pointId)
    {
        var point = _hiddenPoints.FirstOrDefault(point => point.Id == pointId);
        if (point is null)
        {
            throw new NotFoundException($"Hidden point {pointId} does not exist.");
        }
        return point;
    }

    private static void Validate(string name, int xp, Location startLocation)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A location challenge requires a name.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A location challenge requires an XP reward greater than zero.");
        }
        if (startLocation is null)
        {
            throw new DomainException("A location challenge requires a start location.");
        }
    }
}
