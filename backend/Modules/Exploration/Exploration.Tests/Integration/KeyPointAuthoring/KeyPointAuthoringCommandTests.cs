using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringCommandTests : BaseIntegrationTest
{
    public KeyPointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private static AddKeyPointDto NewKeyPoint(Guid imageId) =>
        new("Varadinski most", "Most koji vodi do tvrđave.", imageId, "Tajna mosta.", true, new LocationDto(45.2520, 19.8630));

    private static ChangeKeyPointDto ChangedKeyPoint(Guid imageId) =>
        new("Pristanište brodova", "Pristanište za izletničke brodove.", imageId, null, true, new LocationDto(45.2485, 19.8555));

    [Fact]
    public async Task Author_adds_a_key_point_at_the_end_with_its_image_linked_and_the_length_cleared()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageId = UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.MeasuredDraftQuay.Id).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points",
            NewKeyPoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.MeasuredDraftQuay.Id);
        stored.KeyPoints.Should().HaveCount(keyPointCountBefore + 1);
        var added = stored.KeyPoints.Single(keyPoint => keyPoint.ImageId == imageId);
        added.Order.Should().Be(keyPointCountBefore + 1);
        added.Name.Should().Be("Varadinski most");
        stored.LengthInKm.Should().BeNull();
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeTrue();
    }

    [Fact]
    public async Task Explorer_cannot_add_a_key_point_to_another_authors_tour_and_the_tour_stays_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageId = UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.SecondExplorersTrail.Id}/key-points",
            NewKeyPoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == TourSeed.SecondExplorersTrail.Id).KeyPoints.Should().BeEmpty();
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_key_point()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points",
            NewKeyPoint(UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points",
            NewKeyPoint(UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_a_key_point_with_a_new_image_which_becomes_linked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[1].Id;
        var imageId = UploadedImageSeed.ExplorersUnlinkedSquarePhoto.Id;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}",
            ChangedKeyPoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.MeasuredDraftQuay.Id)
            .KeyPoints.Single(keyPoint => keyPoint.Id == keyPointId);
        stored.Name.Should().Be("Pristanište brodova");
        stored.ImageId.Should().Be(imageId);
        stored.Location.Latitude.Should().Be(45.2485);
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_key_point()
    {
        var client = Factory.CreateClient();
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[1].Id;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}",
            ChangedKeyPoint(UploadedImageSeed.ExplorersUnlinkedSquarePhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[1].Id;

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}",
            ChangedKeyPoint(UploadedImageSeed.ExplorersUnlinkedSquarePhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_removes_a_key_point_and_it_is_no_longer_in_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[0].Id;

        var response = await client.DeleteAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == TourSeed.MeasuredDraftQuay.Id)
            .KeyPoints.Should().NotContain(keyPoint => keyPoint.Id == keyPointId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClient();
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[0].Id;

        var response = await client.DeleteAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var keyPointId = TourSeed.MeasuredDraftQuay.KeyPoints[0].Id;

        var response = await client.DeleteAsync($"/api/exploration/tours/{TourSeed.MeasuredDraftQuay.Id}/key-points/{keyPointId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_records_a_calculated_length_which_is_stored_with_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/length",
            new TourLengthDto(4.75m));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.Tours.Single(tour => tour.Id == TourSeed.FortressWalk.Id).LengthInKm.Should().Be(4.75m);
    }

    [Fact]
    public async Task Anonymous_user_cannot_record_a_length()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/length",
            new TourLengthDto(4.75m));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_record_a_length()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}/length",
            new TourLengthDto(4.75m));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_uploads_an_image_which_is_stored_as_their_unlinked_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var imageCountBefore = arrangeContext.UploadedImages.Count();

        var response = await client.PostAsync("/api/exploration/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        uploaded!.Url.Should().Be(ImageAddress.For(uploaded.Id));
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.UploadedImages.Count().Should().Be(imageCountBefore + 1);
        var stored = assertContext.UploadedImages.Single(image => image.Id == uploaded.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.IsLinked.Should().BeFalse();
        stored.File.Content.Should().Equal(UploadedImageSeed.OnePixelPng);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/exploration/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_upload_an_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/exploration/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent CreateUpload(byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { file, "file", "slika.png" } };
    }
}
