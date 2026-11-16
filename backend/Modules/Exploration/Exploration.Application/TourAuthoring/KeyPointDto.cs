using Exploration.Application.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    string Name,
    string Description,
    Guid ImageId,
    string ImageUrl,
    string? Secret,
    bool IsPublic,
    int Order,
    LocationDto Location);
