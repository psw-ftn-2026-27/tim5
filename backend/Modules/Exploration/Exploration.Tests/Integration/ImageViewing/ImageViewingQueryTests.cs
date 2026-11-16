using System.Net;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Xunit;

namespace Exploration.Tests.Integration.ImageViewing;

public class ImageViewingQueryTests : BaseIntegrationTest
{
    public ImageViewingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Stored_image_is_returned_with_its_content_and_type()
    {
        var client = Factory.CreateClient();
        var imageId = UploadedImageSeed.ExplorersLinkedQuayStartPhoto.Id;

        var response = await client.GetAsync($"/api/exploration/images/{imageId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        (await response.Content.ReadAsByteArrayAsync()).Should().Equal(UploadedImageSeed.OnePixelPng);
    }
}
