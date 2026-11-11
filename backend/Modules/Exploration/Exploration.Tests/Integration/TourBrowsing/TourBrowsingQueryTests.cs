using System.Net.Http.Json;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedSalasi.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Published_tours_are_listed_with_their_recommended_group_size()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        var vineyards = tours!.Items.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        var seeded = TourSeed.PublishedVineyards.RecommendedGroupSize!;
        vineyards.RecommendedGroupSize.Should().Be(new GroupSizeDto(seeded.Min, seeded.Max));
    }

    [Fact]
    public async Task Part_of_a_name_finds_a_tour_regardless_of_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=VINOGRADI", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().Equal(TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Several_difficulties_find_tours_with_any_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Hard", JsonOptions);

        var expected = PublishedSeedTours()
            .Where(tour => tour.Difficulty is TourDifficulty.Easy or TourDifficulty.Hard)
            .Select(tour => tour.Id);
        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Several_tags_find_tours_with_at_least_one_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=vino&tags=istorija", JsonOptions);

        var expected = PublishedSeedTours()
            .Where(tour => tour.Tags.Contains("vino") || tour.Tags.Contains("istorija"))
            .Select(tour => tour.Id);
        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task A_tag_finds_tours_regardless_of_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=ISTORIJA", JsonOptions);

        var expected = PublishedSeedTours()
            .Where(tour => tour.Tags.Contains("istorija"))
            .Select(tour => tour.Id);
        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Several_transport_modes_find_tours_with_a_time_for_at_least_one_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?transports=Bicycle&transports=Car", JsonOptions);

        var expected = PublishedSeedTours()
            .Where(tour => tour.TransportTimes.Any(time => time.Transport is TransportMode.Bicycle or TransportMode.Car))
            .Select(tour => tour.Id);
        tours!.Items.Select(tour => tour.Id).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task A_tour_is_found_only_when_it_meets_every_chosen_criterion()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=grad&difficulties=Easy&tags=istorija&transports=Walking", JsonOptions);

        tours!.Items.Select(tour => tour.Id).Should().Equal(TourSeed.PublishedOldTown.Id);
    }

    [Fact]
    public async Task Tours_sorted_by_ascending_difficulty_go_from_easy_to_hard()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sort=DifficultyAscending", JsonOptions);

        var expected = PublishedSeedTours().Select(tour => tour.Difficulty).OrderBy(difficulty => difficulty);
        tours!.Items.Select(tour => tour.Difficulty).Should().Equal(expected);
    }

    [Fact]
    public async Task Tours_sorted_by_descending_difficulty_go_from_hard_to_easy()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sort=DifficultyDescending", JsonOptions);

        var expected = PublishedSeedTours().Select(tour => tour.Difficulty).OrderByDescending(difficulty => difficulty);
        tours!.Items.Select(tour => tour.Difficulty).Should().Equal(expected);
    }

    [Fact]
    public async Task Second_page_of_a_filtered_search_counts_every_matching_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Moderate&page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(PublishedSeedTours().Count(tour => tour.Difficulty is TourDifficulty.Easy or TourDifficulty.Moderate));
    }

    private static IEnumerable<Tour> PublishedSeedTours()
    {
        return TourSeed.All.Where(tour => tour.Status == TourStatus.Published);
    }
}
