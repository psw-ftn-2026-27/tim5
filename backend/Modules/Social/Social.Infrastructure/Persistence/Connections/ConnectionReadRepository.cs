using Microsoft.EntityFrameworkCore;
using Social.Application.Connections;
using Social.Application.Following;

namespace Social.Infrastructure.Persistence.Connections;

internal sealed class ConnectionReadRepository : IConnectionReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ConnectionReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<FollowRequestDto>> GetByFollowerAsync(Guid followerId)
    {
        return _dbContext.Connections
            .SelectMany(connection => connection.FollowRequests)
            .Where(request => request.FollowerId == followerId)
            .Select(request => new FollowRequestDto(request.FolloweeId, request.Status))
            .ToListAsync();
    }
}
