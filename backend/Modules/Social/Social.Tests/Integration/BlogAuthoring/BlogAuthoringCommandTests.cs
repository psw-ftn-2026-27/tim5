using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Social.Application.UploadedImages;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore + 1);
        var stored = assertContext.Blogs.Single(blog => blog.Id == created.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
        stored.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_cannot_create_a_blog_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("   ", "Opis novog bloga.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_blog()
    {
        var client = Factory.CreateClient();
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", []);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Draft_blog_is_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }

    [Fact]
    public async Task Author_creates_a_blog_whose_images_become_linked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageIds = new List<Guid> { UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id, UploadedImageSeed.ExplorersUnlinkedSquarePhoto.Id };
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", imageIds);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        created!.Images.Should().Equal(imageIds.Select(ImageAddress.For));
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Single(blog => blog.Id == created.Id).ImageIds.Should().Equal(imageIds);
        assertContext.UploadedImages.Where(image => imageIds.Contains(image.Id))
            .Should().HaveCount(2).And.OnlyContain(image => image.IsLinked);
    }

    [Fact]
    public async Task Blog_with_another_users_image_is_not_saved_and_its_images_stay_unlinked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageIds = new List<Guid> { UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id, UploadedImageSeed.SecondExplorersUnlinkedPhoto.Id };
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", imageIds);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
        assertContext.UploadedImages.Where(image => imageIds.Contains(image.Id))
            .Should().HaveCount(2).And.OnlyContain(image => !image.IsLinked);
    }

    [Fact]
    public async Task Author_uploads_an_image_and_gets_its_address_while_the_image_stays_unlinked()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var imageCountBefore = arrangeContext.UploadedImages.Count();

        var response = await client.PostAsync("/api/social/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploaded = await response.Content.ReadFromJsonAsync<UploadedImageDto>(JsonOptions);
        uploaded!.Url.Should().Be(ImageAddress.For(uploaded.Id));
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.UploadedImages.Count().Should().Be(imageCountBefore + 1);
        var stored = assertContext.UploadedImages.Single(image => image.Id == uploaded.Id);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.IsLinked.Should().BeFalse();
        stored.File.Content.Should().Equal(UploadedImageSeed.OnePixelPng);
    }

    [Fact]
    public async Task Rejected_file_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var fakeImage = Encoding.UTF8.GetBytes("Ovo nije slika, iako se predstavlja kao PNG.");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var imageCountBefore = arrangeContext.UploadedImages.Count();

        var response = await client.PostAsync("/api/social/images", CreateUpload(fakeImage));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.UploadedImages.Count().Should().Be(imageCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/social/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_upload_an_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/social/images", CreateUpload(UploadedImageSeed.OnePixelPng));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_deletes_their_own_unlinked_image_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var imageId = UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id;

        var response = await client.DeleteAsync($"/api/social/images/{imageId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.UploadedImages.Should().NotContain(image => image.Id == imageId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_an_image()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/images/{UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_an_image()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/images/{UploadedImageSeed.ExplorersUnlinkedHarborPhoto.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static MultipartFormDataContent CreateUpload(byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        return new MultipartFormDataContent { { file, "file", "slika.png" } };
    }
}
