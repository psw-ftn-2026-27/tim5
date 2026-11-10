using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.FollowerManagement;
using Social.Domain.Connections;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.FollowerManagement;

public class FollowerManagementQueryTests : BaseIntegrationTest
{
    public FollowerManagementQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Followers_overview_contains_only_the_signed_in_users_received_requests_and_followers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var receivedRequests = ConnectionSeed.All
            .SelectMany(connection => connection.FollowRequests)
            .Where(request => request.FolloweeId == WellKnownUsers.Explorer)
            .ToList();
        var expectedPendingRequests = receivedRequests
            .Where(request => request.Status == FollowRequestStatus.Pending)
            .Select(request => request.FollowerId);
        var expectedFollowers = receivedRequests
            .Where(request => request.Status == FollowRequestStatus.Accepted)
            .Select(request => request.FollowerId);

        var followers = await client.GetFromJsonAsync<FollowersDto>("/api/social/followers", JsonOptions);

        followers!.PendingRequests.Select(request => request.UserId).Should().BeEquivalentTo(expectedPendingRequests);
        followers.Followers.Select(follower => follower.UserId).Should().BeEquivalentTo(expectedFollowers);
    }
}
