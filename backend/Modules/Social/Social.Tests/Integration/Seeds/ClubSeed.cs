using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club Mountaineers = new(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), "Vikend usponi i šetnje stazama Fruške gore.", [UploadedImageSeed.ExplorersLinkedMountainPhoto.Id, UploadedImageSeed.ExplorersLinkedLodgePhoto.Id]);
    public static readonly Club Photographers = new(WellKnownUsers.SecondExplorer, new ClubName("Fotografi Novog Sada"), "Zajedničke fotografske šetnje gradom.", [UploadedImageSeed.SecondExplorersLinkedQuayPhoto.Id]);

    public static Club[] All => [Mountaineers, Photographers];
}
