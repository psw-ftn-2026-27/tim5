using Social.Domain.Connections;

namespace Social.Application.Following;

public sealed record FollowingDto(Guid UserId, string? Username, FollowRequestStatus Status);
