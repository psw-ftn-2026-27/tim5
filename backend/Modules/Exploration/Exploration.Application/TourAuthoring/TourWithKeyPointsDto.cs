using Exploration.Domain.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record TourWithKeyPointsDto(
    Guid Id,
    string Name,
    TourStatus Status,
    decimal? LengthInKm,
    List<KeyPointDto> KeyPoints);
