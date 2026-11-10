using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Connections;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingCommandTests : BaseIntegrationTest
{
    public FollowingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_request_between_two_users_creates_a_connection_with_a_pending_request()
    {
        var followerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(followerId, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync($"/api/social/following/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore + 1);
        var stored = assertContext.Connections.Single(connection => connection.FirstUserId == followerId);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == followerId &&
            request.FolloweeId == WellKnownUsers.Explorer &&
            request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Returned_follow_is_recorded_in_the_existing_connection()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.PostAsync($"/api/social/following/{WellKnownUsers.SecondExplorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.Explorer &&
            request.FolloweeId == WellKnownUsers.SecondExplorer &&
            request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task Withdrawn_request_disappears_from_the_connection()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Administrator}/request");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.ExplorerRequestedAdministrator.Id);
        stored.FollowRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task Unfollowing_removes_the_accepted_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().NotContain(request => request.FollowerId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task User_cannot_request_to_follow_again_while_their_request_is_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Connections
            .Single(connection => connection.Id == ConnectionSeed.ExplorerRequestedAdministrator.Id)
            .FollowRequests.Count;

        var response = await client.PostAsync($"/api/social/following/{WellKnownUsers.Administrator}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.ExplorerRequestedAdministrator.Id);
        stored.FollowRequests.Should().HaveCount(requestCountBefore);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.Explorer && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task User_cannot_withdraw_a_request_to_a_user_they_have_no_connection_with()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.DeleteAsync($"/api/social/following/{Guid.NewGuid()}/request");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task Accepted_follow_cannot_be_withdrawn_as_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Explorer}/request");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.SecondExplorerFollowsExplorer.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.SecondExplorer && request.Status == FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task User_cannot_unfollow_a_user_they_have_no_connection_with()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var connectionCountBefore = arrangeContext.Connections.Count();

        var response = await client.DeleteAsync($"/api/social/following/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Connections.Count().Should().Be(connectionCountBefore);
    }

    [Fact]
    public async Task User_cannot_unfollow_a_user_whose_follow_request_is_still_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Connections.Single(connection =>
            connection.Id == ConnectionSeed.ExplorerRequestedAdministrator.Id);
        stored.FollowRequests.Should().ContainSingle(request =>
            request.FollowerId == WellKnownUsers.Explorer && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_to_follow()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/following/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_request_to_follow()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/social/following/{WellKnownUsers.Explorer}", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Administrator}/request");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_withdraw_a_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Explorer}/request");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_unfollow()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_unfollow()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
