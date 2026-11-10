namespace Social.Application.FollowerManagement;

public sealed record FollowersDto(List<FollowerDto> PendingRequests, List<FollowerDto> Followers);
