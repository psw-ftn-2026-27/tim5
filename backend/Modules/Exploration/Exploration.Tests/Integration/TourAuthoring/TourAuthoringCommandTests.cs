using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringCommandTests : BaseIntegrationTest
{
    public TourAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_tour_with_a_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Hard, ["planina"], new GroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<TourDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore + 1);
        var stored = assertContext.Tours.Single(tour => tour.Id == created.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.TransportTimes.Should().BeEmpty();
        stored.RecommendedGroupSize.Should().Be(new GroupSize(2, 6));
    }

    [Fact]
    public async Task Explorer_cannot_create_a_tour_without_a_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateTourDto("   ", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var tourCountBefore = arrangeContext.Tours.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Count().Should().Be(tourCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateTourDto("Nova tura", "Opis nove ture.", TourDifficulty.Easy, ["planina"], new GroupSizeDto(2, 6));

        var response = await client.PostAsJsonAsync("/api/exploration/tours", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_adds_a_transport_time_to_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TransportTimeDto(TransportMode.Walking, 120);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/transport-times",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.TransportTimes.Should().ContainSingle(time => time.Transport == TransportMode.Walking && time.Minutes == 120);
    }

    [Fact]
    public async Task Tour_is_published_when_all_rules_are_met()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_tour()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public async Task Author_archives_their_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Explorer_cannot_archive_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Draft_tour_cannot_be_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.Status.Should().Be(TourStatus.Draft);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_archive_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_reactivates_their_archived_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalasi.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedSalasi.Id);
        stored.Status.Should().Be(TourStatus.Published);
        stored.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task Explorer_cannot_reactivate_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalasi.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.ArchivedSalasi.Id);
        stored.Status.Should().Be(TourStatus.Archived);
        stored.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Published_tour_cannot_be_reactivated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.Status.Should().Be(TourStatus.Published);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalasi.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_reactivate_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.ArchivedSalasi.Id}/reactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_the_recommended_group_size_of_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new GroupSizeDto(3, 10);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/recommended-group-size",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.RecommendedGroupSize.Should().Be(new GroupSize(3, 10));
    }

    [Fact]
    public async Task Invalid_recommended_group_size_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new GroupSizeDto(5, 3);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/recommended-group-size",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id);
        stored.RecommendedGroupSize.Should().Be(TourSeed.FortressWalk.RecommendedGroupSize);
    }

    [Fact]
    public async Task Explorer_cannot_change_the_recommended_group_size_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new GroupSizeDto(3, 10);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.SecondExplorersTrail.Id}/recommended-group-size",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
        stored.RecommendedGroupSize.Should().Be(TourSeed.SecondExplorersTrail.RecommendedGroupSize);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_the_recommended_group_size_of_a_tour()
    {
        var client = Factory.CreateClient();
        var request = new GroupSizeDto(3, 10);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/recommended-group-size",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_the_recommended_group_size_of_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new GroupSizeDto(3, 10);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/recommended-group-size",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
