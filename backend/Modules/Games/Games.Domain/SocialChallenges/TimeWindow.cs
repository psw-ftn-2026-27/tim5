using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record TimeWindow
{
    public TimeOnly Start { get; private init; }
    public TimeOnly End { get; private init; }

    private TimeWindow() { }

    public TimeWindow(TimeOnly start, TimeOnly end)
    {
        if (start == end)
        {
            throw new DomainException("A time window requires a start and an end that differ.");
        }

        Start = start;
        End = end;
    }
}
