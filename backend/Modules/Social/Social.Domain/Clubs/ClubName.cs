using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed record ClubName
{
    public string Value { get; private init; }
    public string NormalizedValue { get; private init; }

    private ClubName()
    {
        Value = null!;
        NormalizedValue = null!;
    }

    public ClubName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("A club requires a name.");
        }

        Value = value.Trim();
        NormalizedValue = Value.ToLowerInvariant();
    }
}
