using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.HiddenPointAuthoring;
using Games.Application.Quizzes;
using Games.Application.UploadedImages;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.HiddenPointAuthoring;

public class HiddenPointAuthoringCommandTests : BaseIntegrationTest
{
    public HiddenPointAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static HiddenPointInputDto CreatePoint(Guid imageId) =>
        new(imageId, new LocationDto(45.2485, 19.8555));

    [Fact]
    public async Task Administrator_uploads_a_photo_which_is_stored_as_their_unlinked_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var imageCountBefore = arrangeContext.UploadedImages.Count();

        var response = await client.PostAsync("/api/games/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        uploaded!.Url.Should().Be(ImageAddress.For(uploaded.Id));
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.UploadedImages.Count().Should().Be(imageCountBefore + 1);
        var stored = assertContext.UploadedImages.Single(image => image.Id == uploaded.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Administrator);
        stored.IsLinked.Should().BeFalse();
        stored.File.Content.Should().Equal(UploadedImageSeed.OnePixelPng);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_a_photo()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/games/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_upload_a_photo()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync("/api/games/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_adds_a_hidden_point_which_is_stored_with_its_photo_linked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id;
        var imageId = UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id;
        var request = CreatePoint(imageId);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var pointCountBefore = arrangeContext.LocationChallenges.Single(challenge => challenge.Id == challengeId).HiddenPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.HiddenPoints.Should().HaveCount(pointCountBefore + 1);
        stored.HiddenPoints.Single(point => point.ImageId == imageId).Location
            .Should().Be(new Location(request.Location.Latitude, request.Location.Longitude));
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeTrue();
    }

    [Fact]
    public async Task Adding_a_hidden_point_with_a_photo_that_does_not_exist_is_rejected_and_the_challenge_gets_no_new_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var pointCountBefore = arrangeContext.LocationChallenges.Single(challenge => challenge.Id == challengeId).HiddenPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points", CreatePoint(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId)
            .HiddenPoints.Should().HaveCount(pointCountBefore);
    }

    [Fact]
    public async Task Adding_a_hidden_point_with_an_already_linked_photo_is_rejected_and_the_challenge_gets_no_new_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id;
        var imageId = UploadedImageSeed.AdministratorsLinkedFortressGatePhoto.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var pointCountBefore = arrangeContext.LocationChallenges.Single(challenge => challenge.Id == challengeId).HiddenPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points", CreatePoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId)
            .HiddenPoints.Should().HaveCount(pointCountBefore);
    }

    [Fact]
    public async Task Adding_a_hidden_point_to_a_challenge_that_does_not_exist_is_rejected_and_the_photo_stays_unlinked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var imageId = UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id;

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{Guid.NewGuid()}/hidden-points", CreatePoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_hidden_point()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id}/hidden-points",
            CreatePoint(UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_add_a_hidden_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftQuayHuntWithoutPoints.Id}/hidden-points",
            CreatePoint(UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_hidden_point_to_a_new_photo_which_becomes_linked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[1].Id;
        var imageId = UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id;
        var request = CreatePoint(imageId);

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId)
            .HiddenPoints.Single(point => point.Id == pointId);
        stored.ImageId.Should().Be(imageId);
        stored.Location.Should().Be(new Location(request.Location.Latitude, request.Location.Longitude));
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeTrue();
    }

    [Fact]
    public async Task Changing_a_hidden_point_of_a_challenge_that_does_not_exist_is_rejected_and_the_photo_stays_unlinked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[1].Id;
        var imageId = UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id;

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{Guid.NewGuid()}/hidden-points/{pointId}", CreatePoint(imageId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.UploadedImages.Single(image => image.Id == imageId).IsLinked.Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_hidden_point()
    {
        var client = Factory.CreateClient();
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[1].Id;

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}",
            CreatePoint(UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_hidden_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[1].Id;

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}",
            CreatePoint(UploadedImageSeed.AdministratorsUnlinkedClockTowerPhoto.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_removes_a_hidden_point_and_it_is_no_longer_in_the_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[0].Id;

        var response = await client.DeleteAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId)
            .HiddenPoints.Should().NotContain(point => point.Id == pointId);
    }

    [Fact]
    public async Task Removing_a_hidden_point_of_a_challenge_that_does_not_exist_is_rejected_and_the_point_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[0].Id;

        var response = await client.DeleteAsync($"/api/games/location-challenges/{Guid.NewGuid()}/hidden-points/{pointId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId)
            .HiddenPoints.Should().Contain(point => point.Id == pointId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_hidden_point()
    {
        var client = Factory.CreateClient();
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[0].Id;

        var response = await client.DeleteAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_remove_a_hidden_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var pointId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.HiddenPoints[0].Id;

        var response = await client.DeleteAsync($"/api/games/location-challenges/{challengeId}/hidden-points/{pointId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent CreateUpload(byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { file, "file", "fotografija.png" } };
    }
}
