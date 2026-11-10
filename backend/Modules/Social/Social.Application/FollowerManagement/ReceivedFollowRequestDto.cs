using Social.Domain.Connections;

namespace Social.Application.FollowerManagement;

public sealed record ReceivedFollowRequestDto(Guid FollowerId, FollowRequestStatus Status);
