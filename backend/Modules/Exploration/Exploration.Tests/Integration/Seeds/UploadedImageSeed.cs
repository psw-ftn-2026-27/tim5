using Shared.Domain.Images;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

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
    public static readonly UploadedImage ExplorersLinkedFishermansIslandPhoto;
    public static readonly UploadedImage ExplorersLinkedLibertyBridgePhoto;
    public static readonly UploadedImage ExplorersLinkedCellarPhoto;
    public static readonly UploadedImage ExplorersLinkedKarlovciSquarePhoto;
    public static readonly UploadedImage ExplorersLinkedKrusedolPhoto;
    public static readonly UploadedImage ExplorersLinkedGrgetegPhoto;
    public static readonly UploadedImage ExplorersLinkedFarmhousePhoto;
    public static readonly UploadedImage ExplorersLinkedWorkshopPhoto;
    public static readonly UploadedImage ExplorersLinkedLibertySquarePhoto;
    public static readonly UploadedImage ExplorersLinkedDanubeParkPhoto;
    public static readonly UploadedImage ExplorersLinkedQuayStartPhoto;
    public static readonly UploadedImage ExplorersLinkedQuayPierPhoto;
    public static readonly UploadedImage ExplorersLinkedQuayEndPhoto;
    public static readonly UploadedImage ExplorersLinkedMarshPhoto;
    public static readonly UploadedImage ExplorersLinkedHeronColonyPhoto;

    static UploadedImageSeed()
    {
        ExplorersLinkedFishermansIslandPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedFishermansIslandPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedLibertyBridgePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedLibertyBridgePhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedCellarPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedCellarPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedKarlovciSquarePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedKarlovciSquarePhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedKrusedolPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedKrusedolPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedGrgetegPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedGrgetegPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedFarmhousePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedFarmhousePhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedWorkshopPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedWorkshopPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedLibertySquarePhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedLibertySquarePhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedDanubeParkPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedDanubeParkPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedQuayStartPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedQuayStartPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedQuayPierPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedQuayPierPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedQuayEndPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedQuayEndPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedMarshPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedMarshPhoto.LinkTo(WellKnownUsers.Explorer);

        ExplorersLinkedHeronColonyPhoto = UploadQuota.CreateImage(WellKnownUsers.Explorer, 0, new ImageFile(OnePixelPng));
        ExplorersLinkedHeronColonyPhoto.LinkTo(WellKnownUsers.Explorer);
    }

    public static UploadedImage[] All =>
    [
        ExplorersUnlinkedHarborPhoto, ExplorersUnlinkedSquarePhoto,
        ExplorersLinkedFishermansIslandPhoto, ExplorersLinkedLibertyBridgePhoto,
        ExplorersLinkedCellarPhoto, ExplorersLinkedKarlovciSquarePhoto,
        ExplorersLinkedKrusedolPhoto, ExplorersLinkedGrgetegPhoto,
        ExplorersLinkedFarmhousePhoto, ExplorersLinkedWorkshopPhoto,
        ExplorersLinkedLibertySquarePhoto, ExplorersLinkedDanubeParkPhoto,
        ExplorersLinkedQuayStartPhoto, ExplorersLinkedQuayPierPhoto, ExplorersLinkedQuayEndPhoto,
        ExplorersLinkedMarshPhoto, ExplorersLinkedHeronColonyPhoto
    ];
}
