using System.Buffers.Binary;
using System.Text;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Xunit;

namespace Social.Tests.Unit.UploadedImages;

public class ImageFileTests
{
    [Theory]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("png", "image/png")]
    [InlineData("webp-lossy", "image/webp")]
    [InlineData("webp-lossless", "image/webp")]
    [InlineData("webp-extended", "image/webp")]
    public void Jpeg_png_and_webp_files_become_images_of_the_format_read_from_their_content(string format, string contentType)
    {
        var content = CreateImage(format, ImageFile.MaxDimensionInPixels, 600);

        var file = new ImageFile(content);

        file.ContentType.Should().Be(contentType);
        file.Size.Should().Be(content.Length);
    }

    [Theory]
    [InlineData("jpeg", 4001, 600)]
    [InlineData("jpeg", 800, 4001)]
    [InlineData("png", 4001, 600)]
    [InlineData("png", 800, 4001)]
    [InlineData("webp-lossy", 4001, 600)]
    [InlineData("webp-lossy", 800, 4001)]
    [InlineData("webp-lossless", 4001, 600)]
    [InlineData("webp-lossless", 800, 4001)]
    [InlineData("webp-extended", 4001, 600)]
    [InlineData("webp-extended", 800, 4001)]
    public void Image_wider_or_taller_than_the_limit_cannot_become_an_image(string format, int width, int height)
    {
        var content = CreateImage(format, width, height);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void File_larger_than_the_size_limit_cannot_become_an_image()
    {
        var content = new byte[ImageFile.MaxSizeInBytes + 1];
        CreatePng(800, 600).CopyTo(content, 0);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Empty_file_cannot_become_an_image()
    {
        var creation = () => new ImageFile([]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("Ovo je tekstualni fajl koji je preimenovan u slika.png.")]
    [InlineData("<?xml version=\"1.0\"?><svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"></svg>")]
    [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>")]
    public void Text_or_svg_file_cannot_become_an_image(string text)
    {
        var content = Encoding.UTF8.GetBytes(text);

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void File_with_only_an_image_signature_cannot_become_an_image()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

        var creation = () => new ImageFile(content);

        creation.Should().Throw<DomainException>();
    }

    private static byte[] CreateImage(string format, int width, int height) => format switch
    {
        "jpeg" => CreateJpeg(width, height),
        "png" => CreatePng(width, height),
        "webp-lossy" => CreateLossyWebP(width, height),
        "webp-lossless" => CreateLosslessWebP(width, height),
        "webp-extended" => CreateExtendedWebP(width, height),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };

    private static byte[] CreateJpeg(int width, int height)
    {
        byte[] jpeg =
        [
            0xFF, 0xD8,
            0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00,
            0xFF, 0xC0, 0x00, 0x11, 0x08, 0, 0, 0, 0, 0x03, 0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01,
            0xFF, 0xD9
        ];
        BinaryPrimitives.WriteUInt16BigEndian(jpeg.AsSpan(25), (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(jpeg.AsSpan(27), (ushort)width);
        return jpeg;
    }

    private static byte[] CreatePng(int width, int height)
    {
        byte[] png =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0, 0, 0, 0, 0, 0, 0, 0, 0x08, 0x06, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ];
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(16), width);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(20), height);
        return png;
    }

    private static byte[] CreateLossyWebP(int width, int height)
    {
        var webP = CreateWebPHeader("VP8 ", 30);
        webP[23] = 0x9D;
        webP[24] = 0x01;
        webP[25] = 0x2A;
        BinaryPrimitives.WriteUInt16LittleEndian(webP.AsSpan(26), (ushort)width);
        BinaryPrimitives.WriteUInt16LittleEndian(webP.AsSpan(28), (ushort)height);
        return webP;
    }

    private static byte[] CreateLosslessWebP(int width, int height)
    {
        var webP = CreateWebPHeader("VP8L", 25);
        webP[20] = 0x2F;
        BinaryPrimitives.WriteUInt32LittleEndian(webP.AsSpan(21), (uint)((width - 1) | ((height - 1) << 14)));
        return webP;
    }

    private static byte[] CreateExtendedWebP(int width, int height)
    {
        var webP = CreateWebPHeader("VP8X", 30);
        BinaryPrimitives.WriteInt32LittleEndian(webP.AsSpan(24), width - 1);
        BinaryPrimitives.WriteInt32LittleEndian(webP.AsSpan(27), height - 1);
        return webP;
    }

    private static byte[] CreateWebPHeader(string chunk, int length)
    {
        var webP = new byte[length + 1];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(webP, 0);
        BinaryPrimitives.WriteInt32LittleEndian(webP.AsSpan(4), length - 8);
        Encoding.ASCII.GetBytes("WEBP").CopyTo(webP, 8);
        Encoding.ASCII.GetBytes(chunk).CopyTo(webP, 12);
        BinaryPrimitives.WriteInt32LittleEndian(webP.AsSpan(16), length - 20);
        return webP;
    }
}
