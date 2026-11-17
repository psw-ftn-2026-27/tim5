using Exploration.Contracts;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration;

public class ExplorationContractTests : BaseIntegrationTest
{
    public ExplorationContractTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Current_position_of_a_user_is_the_one_they_marked()
    {
        var seeded = TouristPositionSeed.SecondExplorerAtLibertySquare;
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var position = await explorationApi.GetCurrentPositionAsync(WellKnownUsers.SecondExplorer);

        position.Should().Be(new UserPositionDto(seeded.Location.Latitude, seeded.Location.Longitude, seeded.MarkedAt));
    }

    [Fact]
    public async Task Current_position_of_a_user_who_never_marked_one_does_not_exist()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var position = await explorationApi.GetCurrentPositionAsync(WellKnownUsers.Explorer);

        position.Should().BeNull();
    }

    [Fact]
    public async Task Only_published_tours_are_confirmed_among_the_requested_ones()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var published = await explorationApi.GetPublishedAsync(
            [TourSeed.PublishedVineyards.Id, TourSeed.CityStroll.Id, TourSeed.ArchivedSalasi.Id, Guid.NewGuid()]);

        published.Should().BeEquivalentTo([TourSeed.PublishedVineyards.Id]);
    }

    [Fact]
    public async Task A_published_tour_requested_twice_is_confirmed_once()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var published = await explorationApi.GetPublishedAsync([TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Id]);

        published.Should().BeEquivalentTo([TourSeed.PublishedVineyards.Id]);
    }

    [Fact]
    public async Task No_requested_tours_confirm_no_published_tours()
    {
        using var scope = Factory.Services.CreateScope();
        var explorationApi = scope.ServiceProvider.GetRequiredService<IExplorationApi>();

        var published = await explorationApi.GetPublishedAsync([]);

        published.Should().BeEmpty();
    }
}
