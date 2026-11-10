using Identity.Contracts;
using Social.Application.Connections;
using Social.Domain.Connections;

namespace Social.Application.FollowerManagement;

public sealed class FollowerManagementQueries
{
    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IIdentityApi _identityApi;

    public FollowerManagementQueries(IConnectionReadRepository connectionReadRepository, IIdentityApi identityApi)
    {
        _connectionReadRepository = connectionReadRepository;
        _identityApi = identityApi;
    }

    public async Task<FollowersDto> GetFollowersAsync(Guid userId)
    {
        var requests = await _connectionReadRepository.GetByFolloweeAsync(userId);
        var users = await _identityApi.GetByIdsAsync(requests.Select(request => request.FollowerId).ToList());

        var pendingRequests = requests
            .Where(request => request.Status == FollowRequestStatus.Pending)
            .Select(request => new FollowerDto(
                request.FollowerId,
                users.FirstOrDefault(user => user.UserId == request.FollowerId)?.Username))
            .ToList();
        var followers = requests
            .Where(request => request.Status == FollowRequestStatus.Accepted)
            .Select(request => new FollowerDto(
                request.FollowerId,
                users.FirstOrDefault(user => user.UserId == request.FollowerId)?.Username))
            .ToList();

        return new FollowersDto(pendingRequests, followers);
    }
}
