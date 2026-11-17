using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product PaidTour = new(
        Guid.Parse("00000000-0000-0000-0004-000000000001"), "Petrovaradinska tvrdjava", WellKnownUsers.Explorer, new Price(150));

    public static readonly Product FreeTour = new(
        Guid.Parse("00000000-0000-0000-0004-000000000002"), "Dunavski kej", WellKnownUsers.Explorer, new Price(0));

    public static readonly Product SecondPaidTour = new(
        Guid.Parse("00000000-0000-0000-0004-000000000004"), "Novosadska sinagoga", WellKnownUsers.Explorer, new Price(200));

    public static readonly Product OtherOwnersTour = new(
        Guid.Parse("00000000-0000-0000-0004-000000000005"), "Sremski Karlovci", WellKnownUsers.SecondExplorer, new Price(180));

    public static Product[] All => [PaidTour, FreeTour, SecondPaidTour, OtherOwnersTour];
}
