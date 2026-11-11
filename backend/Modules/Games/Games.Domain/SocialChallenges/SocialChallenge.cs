using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    public string Name { get; private set; }
    public string ExitMessage { get; private set; }
    public int RequiredPeople { get; private set; }
    public int RangeInMeters { get; private set; }
    public int Xp { get; private set; }
    public TimeWindow Window { get; private set; }
    public ChallengeStatus Status { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        ExitMessage = null!;
        Window = null!;
    }

    public SocialChallenge(string name, string exitMessage, int requiredPeople, int rangeInMeters, int xp, TimeWindow window)
        : base(Guid.NewGuid())
    {
        Validate(name, exitMessage, requiredPeople, rangeInMeters, xp, window);

        Name = name;
        ExitMessage = exitMessage;
        RequiredPeople = requiredPeople;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        Window = window;
        Status = ChallengeStatus.Draft;
    }

    public void Update(string name, string exitMessage, int requiredPeople, int rangeInMeters, int xp, TimeWindow window)
    {
        if (!CanModify())
        {
            throw new DomainException("Only a draft challenge can be changed.");
        }
        Validate(name, exitMessage, requiredPeople, rangeInMeters, xp, window);

        Name = name;
        ExitMessage = exitMessage;
        RequiredPeople = requiredPeople;
        RangeInMeters = rangeInMeters;
        Xp = xp;
        Window = window;
    }

    private bool CanModify()
    {
        return Status == ChallengeStatus.Draft;
    }

    private static void Validate(string name, string exitMessage, int requiredPeople, int rangeInMeters, int xp, TimeWindow window)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A challenge requires a name.");
        }
        if (string.IsNullOrWhiteSpace(exitMessage))
        {
            throw new DomainException("A challenge requires an exit message.");
        }
        if (requiredPeople < 1)
        {
            throw new DomainException("A challenge requires at least one person.");
        }
        if (rangeInMeters <= 0)
        {
            throw new DomainException("A challenge requires a range greater than zero meters.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A challenge requires an XP reward greater than zero.");
        }
        if (window is null)
        {
            throw new DomainException("A challenge requires a time window.");
        }
    }
}
