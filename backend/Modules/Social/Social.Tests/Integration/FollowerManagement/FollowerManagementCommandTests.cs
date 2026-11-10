using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Connections;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowerManagement;

public class FollowerManagementCommandTests : BaseIntegrationTest
{
    public FollowerManagementCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Accepting_a_request_records_it_as_accepted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var followerId = ConnectionSeed.NewcomerRequestedExplorer.FirstUserId;

        var response = await client.PostAsync($"/api/social/followers/{followerId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.NewcomerRequestedExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == followerId &&
            request.FolloweeId == WellKnownUsers.Explorer &&
            request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task Rejecting_a_request_records_it_as_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var followerId = ConnectionSeed.NewcomerRequestedExplorer.FirstUserId;

        var response = await client.PostAsync($"/api/social/followers/{followerId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.NewcomerRequestedExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == followerId &&
            request.FolloweeId == WellKnownUsers.Explorer &&
            request.Status == FollowRequestStatus.Rejected);
    }

    [Fact]
    public async Task Removing_a_follower_deletes_the_accepted_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().NotContain(request => request.FollowerId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task User_cannot_accept_a_request_from_a_user_they_have_no_connection_with()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync($"/api/social/followers/{Guid.NewGuid()}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task Already_accepted_request_cannot_be_accepted_again()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Connections
            .Single(connection => connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id)
            .FollowRequests.Count;

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().HaveCount(requestCountBefore);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.SecondExplorer && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task User_cannot_reject_a_request_from_a_user_they_have_no_connection_with()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync($"/api/social/followers/{Guid.NewGuid()}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task Accepted_request_cannot_be_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.SecondExplorer && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task User_cannot_remove_a_follower_they_have_no_connection_with()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.DeleteAsync($"/api/social/followers/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task Sender_of_a_pending_request_cannot_be_removed_as_a_follower()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var followerId = ConnectionSeed.NewcomerRequestedExplorer.FirstUserId;

        var response = await client.DeleteAsync($"/api/social/followers/{followerId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.NewcomerRequestedExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == followerId && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_accept_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.Explorer}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reject_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/followers/{WellKnownUsers.Explorer}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_follower()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/followers/{WellKnownUsers.Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
