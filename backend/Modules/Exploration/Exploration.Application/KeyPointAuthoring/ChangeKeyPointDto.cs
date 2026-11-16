using Exploration.Application.Tours;

namespace Exploration.Application.KeyPointAuthoring;

public sealed record ChangeKeyPointDto(
    string Name,
    string Description,
    Guid ImageId,
    string? Secret,
    bool IsPublic,
    LocationDto Location);
