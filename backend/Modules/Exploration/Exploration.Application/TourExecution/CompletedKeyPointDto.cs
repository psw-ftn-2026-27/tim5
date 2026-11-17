namespace Exploration.Application.TourExecution;

public sealed record CompletedKeyPointDto(Guid KeyPointId, string Name, DateTime ReachedAt, string? Secret);
