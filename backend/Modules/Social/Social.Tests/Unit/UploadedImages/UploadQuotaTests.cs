using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Shared.Tests;
using Xunit;

namespace Social.Tests.Unit.UploadedImages;

public class UploadQuotaTests
{
    private static readonly byte[] PngHeader =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
        0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x06, 0x00, 0x00, 0x00
    ];

    [Fact]
    public void User_with_the_maximum_number_of_unlinked_images_cannot_upload_another()
    {
        var file = new ImageFile(PngHeader);

        var upload = () => UploadQuota.CreateImage(WellKnownUsers.Explorer, UploadQuota.MaxUnlinkedImages, file);

        upload.Should().Throw<DomainException>();
    }
}
