using Shared.Domain.Exceptions;
using Social.Application.Connections;
using Social.Domain.Connections;

namespace Social.Application.FollowerManagement;

public sealed class FollowerManagementService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowerManagementService(IConnectionRepository connectionRepository, IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AcceptRequestAsync(Guid followeeId, Guid followerId)
    {
        var connection = await GetConnectionAsync(followeeId, followerId);

        connection.Accept(followeeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(Guid followeeId, Guid followerId)
    {
        var connection = await GetConnectionAsync(followeeId, followerId);

        connection.Reject(followeeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFollowerAsync(Guid followeeId, Guid followerId)
    {
        var connection = await GetConnectionAsync(followeeId, followerId);

        connection.RemoveFollower(followeeId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Connection> GetConnectionAsync(Guid followeeId, Guid followerId)
    {
        var connection = await _connectionRepository.GetByUsersAsync(followeeId, followerId);
        if (connection is null)
        {
            throw new NotFoundException($"Connection with user {followerId} does not exist.");
        }
        return connection;
    }
}
