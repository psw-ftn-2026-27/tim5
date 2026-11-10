using Social.Application.FollowerManagement;
using Social.Application.Following;

namespace Social.Application.Connections;

public interface IConnectionReadRepository
{
    Task<List<FollowRequestDto>> GetByFollowerAsync(Guid followerId);
    Task<List<ReceivedFollowRequestDto>> GetByFolloweeAsync(Guid followeeId);
}
