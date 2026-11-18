using Shared.Domain.Images;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

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

    public static readonly UploadedImage AdministratorsUnlinkedClockTowerPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
    public static readonly UploadedImage AdministratorsLinkedFortressGatePhoto;
    public static readonly UploadedImage AdministratorsLinkedUndergroundTunnelPhoto;
    public static readonly UploadedImage AdministratorsLinkedArtistCellarPhoto;
    public static readonly UploadedImage AdministratorsLinkedQuayStairsPhoto;
    public static readonly UploadedImage AdministratorsLinkedPowderMagazinePhoto;
    public static readonly UploadedImage AdministratorsLinkedBastionWallPhoto;
    public static readonly UploadedImage AdministratorsLinkedGalleriesPassagePhoto;
    public static readonly UploadedImage AdministratorsLinkedRavelinStepsPhoto;

    static UploadedImageSeed()
    {
        AdministratorsLinkedFortressGatePhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedFortressGatePhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedUndergroundTunnelPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedUndergroundTunnelPhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedArtistCellarPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedArtistCellarPhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedQuayStairsPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedQuayStairsPhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedPowderMagazinePhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedPowderMagazinePhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedBastionWallPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedBastionWallPhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedGalleriesPassagePhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedGalleriesPassagePhoto.LinkTo(WellKnownUsers.Administrator);

        AdministratorsLinkedRavelinStepsPhoto = UploadQuota.CreateImage(WellKnownUsers.Administrator, 0, new ImageFile(OnePixelPng));
        AdministratorsLinkedRavelinStepsPhoto.LinkTo(WellKnownUsers.Administrator);
    }

    public static UploadedImage[] All =>
    [
        AdministratorsUnlinkedClockTowerPhoto,
        AdministratorsLinkedFortressGatePhoto,
        AdministratorsLinkedUndergroundTunnelPhoto,
        AdministratorsLinkedArtistCellarPhoto,
        AdministratorsLinkedQuayStairsPhoto,
        AdministratorsLinkedPowderMagazinePhoto,
        AdministratorsLinkedBastionWallPhoto,
        AdministratorsLinkedGalleriesPassagePhoto,
        AdministratorsLinkedRavelinStepsPhoto
    ];
}
