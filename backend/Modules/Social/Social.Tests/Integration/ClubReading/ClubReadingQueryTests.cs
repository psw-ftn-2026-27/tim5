using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.ClubReading;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubReading;

public class ClubReadingQueryTests : BaseIntegrationTest
{
    public ClubReadingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Club_overview_lists_the_seeded_clubs_with_their_images()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs?pageSize=100", JsonOptions);

        clubs!.TotalCount.Should().Be(ClubSeed.All.Length);
        foreach (var seeded in ClubSeed.All)
        {
            var listed = clubs.Items.Single(club => club.Id == seeded.Id);
            listed.Name.Should().Be(seeded.Name.Value);
            listed.Description.Should().Be(seeded.Description);
            listed.Images.Should().Equal(seeded.Images.Select(image => image.Url));
        }
    }
}
