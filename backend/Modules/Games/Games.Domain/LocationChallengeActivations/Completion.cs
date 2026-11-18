namespace Games.Domain.LocationChallengeActivations;

// The time and the XP are recorded together, so a completed challenge always carries the XP it earned.
public sealed record Completion
{
    public DateTime CompletedAt { get; private init; }
    public int Xp { get; private init; }

    private Completion() { }

    public Completion(DateTime completedAt, int xp)
    {
        CompletedAt = completedAt;
        Xp = xp;
    }
}
