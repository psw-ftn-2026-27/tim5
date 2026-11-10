using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Connections;

public sealed class FollowRequest : Entity
{
    public Guid FollowerId { get; private set; }
    public Guid FolloweeId { get; private set; }
    public FollowRequestStatus Status { get; private set; }

    private FollowRequest() { }

    public FollowRequest(Guid followerId, Guid followeeId) : base(Guid.NewGuid())
    {
        FollowerId = followerId;
        FolloweeId = followeeId;
        Status = FollowRequestStatus.Pending;
    }

    public void Accept()
    {
        if (Status != FollowRequestStatus.Pending)
        {
            throw new DomainException("Only a pending follow request can be accepted.");
        }

        Status = FollowRequestStatus.Accepted;
    }
}
