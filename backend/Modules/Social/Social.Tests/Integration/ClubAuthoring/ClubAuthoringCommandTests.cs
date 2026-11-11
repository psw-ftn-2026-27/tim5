using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubAuthoring;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubAuthoring;

public class ClubAuthoringCommandTests : BaseIntegrationTest
{
    public ClubAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_founds_a_club_and_becomes_its_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new CreateClubDto("Biciklisti Bačke", "Vožnje bicikla kroz ravnicu.", ["https://example.com/bicikl.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == createdId);
        stored.OwnerId.Should().Be(WellKnownUsers.SecondExplorer);
        stored.Name.Value.Should().Be("Biciklisti Bačke");
        stored.Images.Should().ContainSingle().Which.Url.Should().Be("https://example.com/bicikl.jpg");
    }

    [Fact]
    public async Task Founding_a_club_under_an_existing_name_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new CreateClubDto("  planinari FRUŠKE gore ", "Još jedan klub planinara.", ["https://example.com/planina.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_found_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Biciklisti Bačke", "Vožnje bicikla kroz ravnicu.", ["https://example.com/bicikl.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Owner_updates_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Vojvodine", "Usponi po celoj Vojvodini.", ["https://example.com/vrsac.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.Mountaineers.Id);
        stored.Name.Value.Should().Be("Planinari Vojvodine");
        stored.Description.Should().Be("Usponi po celoj Vojvodini.");
        stored.Images.Should().ContainSingle().Which.Url.Should().Be("https://example.com/vrsac.jpg");
    }

    [Fact]
    public async Task User_who_is_not_the_owner_cannot_update_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new UpdateClubDto("Preuzet klub", "Tuđi opis.", ["https://example.com/tudja.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.Mountaineers.Id);
        stored.Name.Value.Should().Be(ClubSeed.Mountaineers.Name.Value);
        stored.Description.Should().Be(ClubSeed.Mountaineers.Description);
    }

    [Fact]
    public async Task Unknown_club_cannot_be_updated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Nepostojeći klub", "Opis.", ["https://example.com/slika.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Name.Value == "Nepostojeći klub").Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Planinari Vojvodine", "Usponi po celoj Vojvodini.", ["https://example.com/vrsac.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Owner_deletes_their_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == ClubSeed.Mountaineers.Id).Should().BeFalse();
    }

    [Fact]
    public async Task User_who_is_not_the_owner_cannot_delete_the_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == ClubSeed.Mountaineers.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Unknown_club_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.DeleteAsync($"/api/social/clubs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.Mountaineers.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
