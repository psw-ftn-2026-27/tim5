using Social.Application.UploadedImages;

namespace Social.Application.ClubReading;

public sealed record ClubDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Description,
    List<UploadedImageDto> Images);
