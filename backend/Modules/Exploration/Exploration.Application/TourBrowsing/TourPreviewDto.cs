using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourPreviewDto(
    Guid Id,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    decimal? LengthInKm,
    List<TransportTimeDto> TransportTimes,
    List<PublicKeyPointDto> PublicKeyPoints,
    bool IsPurchased,
    bool HasActiveExecution);
