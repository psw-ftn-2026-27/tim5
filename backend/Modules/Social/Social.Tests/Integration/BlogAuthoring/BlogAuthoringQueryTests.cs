using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Blogs;
using Social.Application.UploadedImages;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringQueryTests : BaseIntegrationTest
{
    public BlogAuthoringQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_lists_only_their_own_blogs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blogs = await client.GetFromJsonAsync<List<BlogDto>>("/api/social/blogs/mine", JsonOptions);

        blogs.Should().HaveCount(BlogSeed.All.Count(blog => blog.AuthorId == WellKnownUsers.Explorer));
        blogs.Should().OnlyContain(blog => blog.AuthorId == WellKnownUsers.Explorer);
        blogs.Should().NotContain(blog => blog.Id == BlogSeed.SecondExplorersDiary.Id);
    }

    [Fact]
    public async Task Authors_blogs_are_listed_with_their_image_addresses()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var blogs = await client.GetFromJsonAsync<List<BlogDto>>("/api/social/blogs/mine", JsonOptions);

        blogs!.Single(blog => blog.Id == BlogSeed.IllustratedBridgeWalk.Id).Images
            .Should().Equal(ImageAddress.For(UploadedImageSeed.ExplorersLinkedBridgePhoto.Id));
    }
}
