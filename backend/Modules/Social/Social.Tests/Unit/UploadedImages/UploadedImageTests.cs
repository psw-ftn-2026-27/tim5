using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Shared.Tests;
using Xunit;

namespace Social.Tests.Unit.UploadedImages;

public class UploadedImageTests
{
    private static readonly byte[] PngHeader =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00
    ];

    private static UploadedImage CreateImage() =>
        UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(PngHeader));

    private static UploadedImage CreateLinkedImage()
    {
        var image = CreateImage();
        image.LinkTo(WellKnownUsers.Explorer);
        return image;
    }

    [Fact]
    public void Uploaded_images_of_the_same_file_get_distinct_server_generated_names()
    {
        var first = CreateImage();

        var second = CreateImage();

        first.StoredName.Should().NotBeNullOrWhiteSpace();
        second.StoredName.Should().NotBe(first.StoredName);
    }

    [Fact]
    public void Another_users_image_cannot_be_linked()
    {
        var image = CreateImage();

        var linking = () => image.LinkTo(WellKnownUsers.SecondExplorer);

        linking.Should().Throw<DomainException>();
    }

    [Fact]
    public void Already_linked_image_cannot_be_linked_again()
    {
        var image = CreateLinkedImage();

        var linking = () => image.LinkTo(WellKnownUsers.Explorer);

        linking.Should().Throw<DomainException>();
    }

    [Fact]
    public void Another_users_image_cannot_be_deleted()
    {
        var image = CreateImage();

        var deletion = () => image.Delete(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Linked_image_cannot_be_deleted()
    {
        var image = CreateLinkedImage();

        var deletion = () => image.Delete(WellKnownUsers.Explorer);

        deletion.Should().Throw<DomainException>();
    }
}
