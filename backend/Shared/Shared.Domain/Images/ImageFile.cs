using System.Buffers.Binary;
using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed record ImageFile
{
    public const int MaxSizeInBytes = 5 * 1024 * 1024;
    public const int MaxDimensionInPixels = 4000;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    public byte[] Content { get; private init; }
    public int Size { get; private init; }
    public string ContentType { get; private init; }

    private ImageFile()
    {
        Content = null!;
        ContentType = null!;
    }

    public ImageFile(byte[] content)
    {
        if (content.Length == 0)
        {
            throw new DomainException("An image file cannot be empty.");
        }
        if (content.Length > MaxSizeInBytes)
        {
            throw new DomainException("An image file cannot be larger than 5 MB.");
        }

        var (contentType, width, height) = ReadHeader(content);
        if (width > MaxDimensionInPixels || height > MaxDimensionInPixels)
        {
            throw new DomainException("An image cannot be wider or taller than 4000 pixels.");
        }

        Content = content;
        Size = content.Length;
        ContentType = contentType;
    }

    private static (string ContentType, long Width, long Height) ReadHeader(byte[] content)
    {
        if (HasBytesAt(content, 0, PngSignature))
        {
            var (width, height) = ReadPngDimensions(content);
            return ("image/png", width, height);
        }
        if (HasBytesAt(content, 0, JpegSignature))
        {
            var (width, height) = ReadJpegDimensions(content);
            return ("image/jpeg", width, height);
        }
        if (HasBytesAt(content, 0, "RIFF"u8.ToArray()) && HasBytesAt(content, 8, "WEBP"u8.ToArray()))
        {
            var (width, height) = ReadWebPDimensions(content);
            return ("image/webp", width, height);
        }

        throw new DomainException("Only JPEG, PNG and WebP images are accepted.");
    }

    // PNG: the first chunk is IHDR, holding the width and height as 4-byte big-endian numbers.
    private static (long Width, long Height) ReadPngDimensions(byte[] content)
    {
        if (content.Length < 24 || !HasBytesAt(content, 12, "IHDR"u8.ToArray()))
        {
            throw Damaged();
        }

        return (BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(16)),
            BinaryPrimitives.ReadUInt32BigEndian(content.AsSpan(20)));
    }

    // JPEG: a sequence of segments (0xFF, marker, 2-byte length); a start-of-frame segment
    // holds the height and then the width as 2-byte big-endian numbers.
    private static (long Width, long Height) ReadJpegDimensions(byte[] content)
    {
        var offset = 2;
        while (offset + 4 <= content.Length)
        {
            if (content[offset] != 0xFF)
            {
                throw Damaged();
            }

            var marker = content[offset + 1];
            if (marker == 0xFF)
            {
                offset++;
                continue;
            }
            if (marker == 0x01 || marker is >= 0xD0 and <= 0xD8)
            {
                offset += 2;
                continue;
            }
            if (marker == 0xD9 || marker == 0xDA)
            {
                throw Damaged();
            }

            var length = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(offset + 2));
            if (IsJpegStartOfFrame(marker))
            {
                if (offset + 9 > content.Length)
                {
                    throw Damaged();
                }
                return (BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(offset + 7)),
                    BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(offset + 5)));
            }
            if (length < 2)
            {
                throw Damaged();
            }

            offset += 2 + length;
        }

        throw Damaged();
    }

    private static bool IsJpegStartOfFrame(byte marker) =>
        marker is >= 0xC0 and <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;

    // WebP: a RIFF container whose first chunk is VP8 (lossy), VP8L (lossless) or VP8X (extended),
    // each storing the dimensions in its own little-endian layout.
    private static (long Width, long Height) ReadWebPDimensions(byte[] content)
    {
        if (HasBytesAt(content, 12, "VP8 "u8.ToArray()) && content.Length >= 30
            && HasBytesAt(content, 23, [0x9D, 0x01, 0x2A]))
        {
            return (BinaryPrimitives.ReadUInt16LittleEndian(content.AsSpan(26)) & 0x3FFF,
                BinaryPrimitives.ReadUInt16LittleEndian(content.AsSpan(28)) & 0x3FFF);
        }
        if (HasBytesAt(content, 12, "VP8L"u8.ToArray()) && content.Length >= 25 && content[20] == 0x2F)
        {
            var bits = BinaryPrimitives.ReadUInt32LittleEndian(content.AsSpan(21));
            return ((bits & 0x3FFF) + 1, ((bits >> 14) & 0x3FFF) + 1);
        }
        if (HasBytesAt(content, 12, "VP8X"u8.ToArray()) && content.Length >= 30)
        {
            return (ReadUInt24LittleEndian(content, 24) + 1, ReadUInt24LittleEndian(content, 27) + 1);
        }

        throw Damaged();
    }

    private static long ReadUInt24LittleEndian(byte[] content, int offset) =>
        content[offset] | (content[offset + 1] << 8) | (content[offset + 2] << 16);

    private static bool HasBytesAt(byte[] content, int offset, byte[] expected) =>
        content.Length >= offset + expected.Length
        && content.AsSpan(offset, expected.Length).SequenceEqual(expected);

    private static DomainException Damaged() => new("The image file is damaged.");
}
