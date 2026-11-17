namespace Exploration.Application.TourBrowsing;

// A key point the buyer completed in any of their executions, the only kind whose secret the preview shows.
public sealed record CompletedKeyPointSecretDto(Guid KeyPointId, string Name, string? Secret);
