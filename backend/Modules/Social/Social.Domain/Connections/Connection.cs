using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Connections;

public sealed class Connection : AggregateRoot
{
    public Guid FirstUserId { get; private set; }
    public Guid SecondUserId { get; private set; }
    private readonly List<FollowRequest> _followRequests = [];
    public IReadOnlyList<FollowRequest> FollowRequests => _followRequests;

    private Connection() { }

    public Connection(Guid followerId, Guid followeeId) : base(Guid.NewGuid())
    {
        if (followerId == followeeId)
        {
            throw new DomainException("A user cannot follow themselves.");
        }

        FirstUserId = followerId;
        SecondUserId = followeeId;
        _followRequests.Add(new FollowRequest(followerId, followeeId));
    }

    public void RequestFollow(Guid followerId)
    {
        var followeeId = GetOtherUserId(followerId);
        var ownRequest = FindRequestFrom(followerId);
        var reverseRequest = FindRequestFrom(followeeId);

        if (ownRequest?.Status == FollowRequestStatus.Pending)
        {
            throw new DomainException("A follow request to this user is already pending.");
        }
        if (ownRequest?.Status == FollowRequestStatus.Accepted)
        {
            throw new DomainException("You already follow this user.");
        }
        if (reverseRequest?.Status == FollowRequestStatus.Pending)
        {
            throw new DomainException("This user has sent you a follow request. Respond to it first.");
        }

        if (ownRequest is not null)
        {
            _followRequests.Remove(ownRequest);
        }

        var request = new FollowRequest(followerId, followeeId);
        if (reverseRequest?.Status == FollowRequestStatus.Accepted)
        {
            request.Accept();
        }
        _followRequests.Add(request);
    }

    public void WithdrawRequest(Guid followerId)
    {
        var request = GetRequestFrom(followerId);
        if (request.Status != FollowRequestStatus.Pending)
        {
            throw new DomainException("Only a pending follow request can be withdrawn.");
        }

        _followRequests.Remove(request);
    }

    public void Unfollow(Guid followerId)
    {
        var request = GetRequestFrom(followerId);
        if (request.Status != FollowRequestStatus.Accepted)
        {
            throw new DomainException("You do not follow this user.");
        }

        _followRequests.Remove(request);
    }

    public void Accept(Guid followeeId)
    {
        var request = GetRequestTo(followeeId);

        request.Accept();
    }

    public void Reject(Guid followeeId)
    {
        var request = GetRequestTo(followeeId);

        request.Reject();
    }

    public void RemoveFollower(Guid followeeId)
    {
        var request = GetRequestTo(followeeId);
        if (request.Status != FollowRequestStatus.Accepted)
        {
            throw new DomainException("This user does not follow you.");
        }

        _followRequests.Remove(request);
    }

    private Guid GetOtherUserId(Guid userId)
    {
        if (userId == FirstUserId)
        {
            return SecondUserId;
        }
        if (userId == SecondUserId)
        {
            return FirstUserId;
        }
        throw new DomainException("The user is not part of this connection.");
    }

    private FollowRequest? FindRequestFrom(Guid followerId)
    {
        return _followRequests.FirstOrDefault(request => request.FollowerId == followerId);
    }

    private FollowRequest GetRequestFrom(Guid followerId)
    {
        var request = FindRequestFrom(followerId);
        if (request is null)
        {
            throw new NotFoundException("Follow request does not exist.");
        }
        return request;
    }

    private FollowRequest GetRequestTo(Guid followeeId)
    {
        var request = _followRequests.FirstOrDefault(request => request.FolloweeId == followeeId);
        if (request is null)
        {
            throw new NotFoundException("Follow request does not exist.");
        }
        return request;
    }
}
