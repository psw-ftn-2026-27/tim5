namespace Social.Application.UploadedImages;

// The one place that knows the public address an uploaded image is served from.
public static class ImageAddress
{
    public static string For(Guid imageId) => $"/api/social/images/{imageId}";
}
