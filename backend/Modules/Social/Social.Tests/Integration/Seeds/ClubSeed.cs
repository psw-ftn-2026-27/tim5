using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club Mountaineers = new(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), "Vikend usponi i šetnje stazama Fruške gore.", [new ClubImage("https://example.com/fruska-gora.jpg"), new ClubImage("https://example.com/stazarski-dom.jpg")]);
    public static readonly Club Photographers = new(WellKnownUsers.SecondExplorer, new ClubName("Fotografi Novog Sada"), "Zajedničke fotografske šetnje gradom.", [new ClubImage("https://example.com/kej.jpg")]);

    public static Club[] All => [Mountaineers, Photographers];
}
