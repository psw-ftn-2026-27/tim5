using Social.Domain.Connections;

namespace Social.Application.Connections;

public interface IConnectionRepository
{
    Task<Connection?> GetByUsersAsync(Guid firstUserId, Guid secondUserId);
    void Add(Connection connection);
}
