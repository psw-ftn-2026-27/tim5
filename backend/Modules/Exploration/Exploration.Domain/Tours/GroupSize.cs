using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record GroupSize
{
    public int Min { get; private init; }
    public int Max { get; private init; }

    private GroupSize() { }

    public GroupSize(int min, int max)
    {
        if (min < 1)
        {
            throw new DomainException("A group size requires a minimum of at least 1.");
        }
        if (max < min)
        {
            throw new DomainException("A group size requires a maximum that is not less than the minimum.");
        }

        Min = min;
        Max = max;
    }
}
