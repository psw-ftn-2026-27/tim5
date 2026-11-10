using Microsoft.EntityFrameworkCore;
using Social.Application.Connections;
using Social.Domain.Connections;

namespace Social.Infrastructure.Persistence.Connections;

internal sealed class ConnectionRepository : IConnectionRepository
{
    private readonly SocialDbContext _dbContext;

    public ConnectionRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Connection?> GetByUsersAsync(Guid firstUserId, Guid secondUserId)
    {
        return _dbContext.Connections.FirstOrDefaultAsync(connection =>
            (connection.FirstUserId == firstUserId && connection.SecondUserId == secondUserId) ||
            (connection.FirstUserId == secondUserId && connection.SecondUserId == firstUserId));
    }

    public void Add(Connection connection)
    {
        _dbContext.Connections.Add(connection);
    }
}
