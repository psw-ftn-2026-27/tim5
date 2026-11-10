using FluentAssertions;
using Identity.Contracts;
using Identity.Tests.Integration.Seeds;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.Tests.Integration;

public sealed class IdentityContractTests : BaseIntegrationTest
{
    public IdentityContractTests(IdentityApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Users_are_found_by_part_of_their_username_regardless_of_case()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var users = await identityApi.SearchByUsernameAsync("PLOR", 10);

        users.Should().ContainSingle(user => user.UserId == UserSeed.Explorer.Id && user.Username == UserSeed.Explorer.UserName);
    }

    [Fact]
    public async Task Usernames_are_returned_for_the_requested_users()
    {
        using var scope = Factory.Services.CreateScope();
        var identityApi = scope.ServiceProvider.GetRequiredService<IIdentityApi>();

        var users = await identityApi.GetByIdsAsync([UserSeed.Explorer.Id]);

        users.Should().ContainSingle(user => user.UserId == UserSeed.Explorer.Id && user.Username == UserSeed.Explorer.UserName);
    }
}
