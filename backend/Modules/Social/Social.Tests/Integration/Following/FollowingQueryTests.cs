using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Following;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingQueryTests : BaseIntegrationTest
{
    public FollowingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Following_list_contains_only_the_signed_in_users_follows_and_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var expected = ConnectionSeed.All
            .SelectMany(connection => connection.FollowRequests)
            .Where(request => request.FollowerId == WellKnownUsers.SecondExplorer)
            .Select(request => new { UserId = request.FolloweeId, request.Status });

        var following = await client.GetFromJsonAsync<List<FollowingDto>>("/api/social/following", JsonOptions);

        following!.Select(item => new { item.UserId, item.Status }).Should().BeEquivalentTo(expected);
    }
}
