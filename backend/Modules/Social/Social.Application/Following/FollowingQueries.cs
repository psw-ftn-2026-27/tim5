using Identity.Contracts;
using Social.Application.Connections;

namespace Social.Application.Following;

public sealed class FollowingQueries
{
    private const int SearchLimit = 20;

    private readonly IConnectionReadRepository _connectionReadRepository;
    private readonly IIdentityApi _identityApi;

    public FollowingQueries(IConnectionReadRepository connectionReadRepository, IIdentityApi identityApi)
    {
        _connectionReadRepository = connectionReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<UserDto>> SearchUsersAsync(string username)
    {
        var users = await _identityApi.SearchByUsernameAsync(username, SearchLimit);

        return users.Select(user => new UserDto(user.UserId, user.Username)).ToList();
    }

    public async Task<List<FollowingDto>> GetFollowingAsync(Guid userId)
    {
        var requests = await _connectionReadRepository.GetByFollowerAsync(userId);
        var users = await _identityApi.GetByIdsAsync(requests.Select(request => request.FolloweeId).ToList());

        return requests
            .Select(request => new FollowingDto(
                request.FolloweeId,
                users.FirstOrDefault(user => user.UserId == request.FolloweeId)?.Username,
                request.Status))
            .ToList();
    }
}
