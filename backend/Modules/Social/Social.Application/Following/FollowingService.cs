using Shared.Domain.Exceptions;
using Social.Application.Connections;
using Social.Domain.Connections;

namespace Social.Application.Following;

public sealed class FollowingService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowingService(IConnectionRepository connectionRepository, IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RequestFollowAsync(Guid followerId, Guid followeeId)
    {
        var connection = await _connectionRepository.GetByUsersAsync(followerId, followeeId);
        if (connection is null)
        {
            _connectionRepository.Add(new Connection(followerId, followeeId));
        }
        else
        {
            connection.RequestFollow(followerId);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawRequestAsync(Guid followerId, Guid followeeId)
    {
        var connection = await GetConnectionAsync(followerId, followeeId);

        connection.WithdrawRequest(followerId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowAsync(Guid followerId, Guid followeeId)
    {
        var connection = await GetConnectionAsync(followerId, followeeId);

        connection.Unfollow(followerId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Connection> GetConnectionAsync(Guid followerId, Guid followeeId)
    {
        var connection = await _connectionRepository.GetByUsersAsync(followerId, followeeId);
        if (connection is null)
        {
            throw new NotFoundException($"Connection with user {followeeId} does not exist.");
        }
        return connection;
    }
}
