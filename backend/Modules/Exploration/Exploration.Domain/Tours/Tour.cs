using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;
    private const int MinimumKeyPointsForPublishing = 2;

    private readonly List<TransportTime> _transportTimes = [];
    private readonly List<KeyPoint> _keyPoints = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public GroupSize? RecommendedGroupSize { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints;
    public decimal? LengthInKm { get; private set; }

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags, GroupSize groupSize)
        : this(Guid.NewGuid(), authorId, name, description, difficulty, tags, groupSize)
    {
    }

    /// <summary>
    /// Only for the development seed, whose demo tours have fixed IDs that other modules
    /// refer to. Everywhere else the tour generates its own ID.
    /// </summary>
    public Tour(Guid id, Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags, GroupSize groupSize)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }
        if (groupSize is null)
        {
            throw new DomainException("A tour requires a recommended group size.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        RecommendedGroupSize = groupSize;
        Status = TourStatus.Draft;
    }

    public void ChangeRecommendedGroupSize(GroupSize groupSize)
    {
        RecommendedGroupSize = groupSize;
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void Publish(Price price)
    {
        if (price is null)
        {
            throw new DomainException("A tour can be published only with a price.");
        }
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Only a draft tour can be published.");
        }
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }
        if (_keyPoints.Count < MinimumKeyPointsForPublishing)
        {
            throw new DomainException($"A tour can be published only with at least {MinimumKeyPointsForPublishing} key points.");
        }

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void AddKeyPoint(string name, string description, Guid imageId, string? secret, bool isPublic, Location location)
    {
        EnsureKeyPointsCanChange();

        _keyPoints.Add(new KeyPoint(name, description, imageId, secret, isPublic, _keyPoints.Count + 1, location));
        LengthInKm = null;
    }

    public void ChangeKeyPoint(Guid keyPointId, string name, string description, Guid imageId, string? secret, bool isPublic, Location location)
    {
        EnsureKeyPointsCanChange();

        GetKeyPoint(keyPointId).Change(name, description, imageId, secret, isPublic, location);
        LengthInKm = null;
    }

    public void RemoveKeyPoint(Guid keyPointId)
    {
        EnsureKeyPointsCanChange();

        _keyPoints.Remove(GetKeyPoint(keyPointId));
        var order = 1;
        foreach (var keyPoint in _keyPoints.OrderBy(keyPoint => keyPoint.Order))
        {
            keyPoint.ChangeOrder(order);
            order++;
        }
        LengthInKm = null;
    }

    public void RecordLength(decimal km)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("The length can be recorded only for a draft tour.");
        }
        if (km < 0)
        {
            throw new DomainException("A tour length cannot be negative.");
        }

        LengthInKm = km;
    }

    private void EnsureKeyPointsCanChange()
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be changed only while the tour is a draft.");
        }
    }

    private KeyPoint GetKeyPoint(Guid keyPointId)
    {
        var keyPoint = _keyPoints.FirstOrDefault(keyPoint => keyPoint.Id == keyPointId);
        if (keyPoint is null)
        {
            throw new NotFoundException($"Key point {keyPointId} does not exist.");
        }
        return keyPoint;
    }

    public void Archive()
    {
        if (Status != TourStatus.Published)
        {
            throw new DomainException("Only a published tour can be archived.");
        }

        Status = TourStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
        {
            throw new DomainException("Only an archived tour can be reactivated.");
        }

        Status = TourStatus.Published;
        ArchivedAt = null;
    }
}
