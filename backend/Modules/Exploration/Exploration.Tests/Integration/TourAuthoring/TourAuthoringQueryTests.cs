using System.Net.Http.Json;
using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourAuthoring;

public class TourAuthoringQueryTests : BaseIntegrationTest
{
    public TourAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_tours()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<TourDto>>("/api/exploration/tours/mine", JsonOptions);

        tours.Should().HaveCount(TourSeed.All.Count(tour => tour.AuthorId == WellKnownUsers.Explorer));
        tours.Should().OnlyContain(tour => tour.AuthorId == WellKnownUsers.Explorer);
        tours.Should().NotContain(tour => tour.Id == TourSeed.SecondExplorersTrail.Id);
    }

    [Fact]
    public async Task Author_opens_their_tour_and_gets_its_key_points_in_order_with_secrets_and_length()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = TourSeed.MeasuredDraftQuay;

        var tour = await client.GetFromJsonAsync<TourWithKeyPointsDto>($"/api/exploration/tours/mine/{seeded.Id}", JsonOptions);

        tour!.Id.Should().Be(seeded.Id);
        tour.LengthInKm.Should().Be(seeded.LengthInKm);
        tour.KeyPoints.Select(keyPoint => keyPoint.Id)
            .Should().Equal(seeded.KeyPoints.OrderBy(keyPoint => keyPoint.Order).Select(keyPoint => keyPoint.Id));
        tour.KeyPoints.Select(keyPoint => keyPoint.Secret)
            .Should().Equal(seeded.KeyPoints.OrderBy(keyPoint => keyPoint.Order).Select(keyPoint => keyPoint.Secret));
        tour.KeyPoints.Should().OnlyContain(keyPoint => keyPoint.ImageUrl == ImageAddress.For(keyPoint.ImageId));
    }
}
