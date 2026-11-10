using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Connections;
using Xunit;

namespace Social.Tests.Unit.Connections;

public class ConnectionTests
{
    private static Connection CreatePendingRequest() =>
        new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

    private static Connection CreateAcceptedFollow()
    {
        var connection = CreatePendingRequest();
        connection.Accept(WellKnownUsers.SecondExplorer);
        return connection;
    }

    [Fact]
    public void Request_to_a_user_who_already_follows_the_sender_is_accepted_immediately()
    {
        var connection = CreateAcceptedFollow();

        connection.RequestFollow(WellKnownUsers.SecondExplorer);

        connection.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.SecondExplorer &&
            request.FolloweeId == WellKnownUsers.Explorer &&
            request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public void User_cannot_send_a_request_to_a_user_whose_request_to_them_is_pending()
    {
        var connection = CreatePendingRequest();

        var requesting = () => connection.RequestFollow(WellKnownUsers.SecondExplorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_request_to_follow_again_while_their_request_is_pending()
    {
        var connection = CreatePendingRequest();

        var requesting = () => connection.RequestFollow(WellKnownUsers.Explorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_request_to_follow_a_user_they_already_follow()
    {
        var connection = CreateAcceptedFollow();

        var requesting = () => connection.RequestFollow(WellKnownUsers.Explorer);

        requesting.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_follow_themselves()
    {
        var creation = () => new Connection(WellKnownUsers.Explorer, WellKnownUsers.Explorer);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Accepted_request_cannot_be_withdrawn()
    {
        var connection = CreateAcceptedFollow();

        var withdrawal = () => connection.WithdrawRequest(WellKnownUsers.Explorer);

        withdrawal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recipient_accepts_the_request_and_the_sender_follows_them_but_not_the_other_way_around()
    {
        var connection = CreatePendingRequest();

        connection.Accept(WellKnownUsers.SecondExplorer);

        connection.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.Explorer &&
            request.FolloweeId == WellKnownUsers.SecondExplorer &&
            request.Status == FollowRequestStatus.Accepted);
        connection.FollowRequests.Should().NotContain(request => request.FollowerId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public void User_who_did_not_receive_the_request_cannot_accept_it()
    {
        var connection = CreatePendingRequest();

        var acceptance = () => connection.Accept(WellKnownUsers.Explorer);

        acceptance.Should().Throw<NotFoundException>();
    }
}
