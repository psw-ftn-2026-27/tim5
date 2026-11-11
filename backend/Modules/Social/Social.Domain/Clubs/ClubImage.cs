using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public sealed record ClubImage
{
    public string Url { get; private init; }

    private ClubImage()
    {
        Url = null!;
    }

    public ClubImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new DomainException("A club image requires a URL.");
        }

        Url = url;
    }
}
