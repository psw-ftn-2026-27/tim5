using Shared.Domain.Images;
using Shared.Tests;

namespace Social.Tests.Integration.Seeds;

internal static class UploadedImageSeed
{
    // A complete, valid 1x1 PNG.
    public static readonly byte[] OnePixelPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
        0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
        0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82
    ];

    public static readonly UploadedImage ExplorersUnlinkedHarborPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
    public static readonly UploadedImage ExplorersUnlinkedSquarePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
    public static readonly UploadedImage SecondExplorersUnlinkedPhoto = UploadQuota.CreateImage(WellKnownUsers.SecondExplorer, 0, new ImageFile(OnePixelPng));
    public static readonly UploadedImage ExplorersLinkedBridgePhoto;
    public static readonly UploadedImage ExplorersLinkedMountainPhoto;
    public static readonly UploadedImage ExplorersLinkedLodgePhoto;
    public static readonly UploadedImage SecondExplorersLinkedQuayPhoto;

    static UploadedImageSeed()
    {
        ExplorersLinkedBridgePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedBridgePhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedMountainPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedMountainPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedLodgePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedLodgePhoto.LinkTo(WellKnownUsers.Explorer);

        SecondExplorersLinkedQuayPhoto = UploadQuota.CreateImage(WellKnownUsers.SecondExplorer, 0, new ImageFile(OnePixelPng));
        SecondExplorersLinkedQuayPhoto.LinkTo(WellKnownUsers.SecondExplorer);
    }

    public static UploadedImage[] All => [ExplorersUnlinkedHarborPhoto, ExplorersUnlinkedSquarePhoto, SecondExplorersUnlinkedPhoto, ExplorersLinkedBridgePhoto, ExplorersLinkedMountainPhoto, ExplorersLinkedLodgePhoto, SecondExplorersLinkedQuayPhoto];
}
