using Social.Domain.Connections;

namespace Social.Application.Following;

public sealed record FollowRequestDto(Guid FolloweeId, FollowRequestStatus Status);
