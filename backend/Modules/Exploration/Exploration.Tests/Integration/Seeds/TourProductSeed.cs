using Payment.Domain.Products;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

// Payment's products for the seeded tours, so tour browsing gets real prices through IPaymentApi.
// PublishedMarshWithPublicSecret deliberately has no product: it is not for sale.
// Named TourProductSeed, not ProductSeed, because Payment.Tests sees this namespace next to its own ProductSeed.
internal static class TourProductSeed
{
    public static readonly Product PaidVineyards = new(
        TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, WellKnownUsers.Explorer, new Price(150));

    public static readonly Product MoreExpensiveMonasteries = new(
        TourSeed.PublishedMonasteries.Id, TourSeed.PublishedMonasteries.Name, WellKnownUsers.Explorer, new Price(300));

    public static readonly Product FreeOldTown = new(
        TourSeed.PublishedOldTown.Id, TourSeed.PublishedOldTown.Name, WellKnownUsers.Explorer, new Price(0));

    public static Product[] All => [PaidVineyards, MoreExpensiveMonasteries, FreeOldTown];
}
