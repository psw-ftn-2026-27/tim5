namespace Social.Tests.Integration.Seeds;

internal static class SocialSeed
{
    public static object[] All => [.. BlogSeed.All, .. ConnectionSeed.All, .. UploadedImageSeed.All, .. ClubSeed.All];
}
