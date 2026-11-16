using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// A key point as a tourist sees it before buying: deliberately without the secret.
public sealed record PublicKeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    LocationDto Location);
