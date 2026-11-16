using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly ShoppingCart SecondExplorersCartWithPaidTour = new(WellKnownUsers.SecondExplorer);

    public static readonly ShoppingCart CartWithTwoTours = new(Guid.Parse("00000000-0000-0000-0005-000000000001"));

    static ShoppingCartSeed()
    {
        SecondExplorersCartWithPaidTour.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId);
        CartWithTwoTours.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId);
        CartWithTwoTours.AddItem(
            ProductSeed.SecondPaidTour.EntityId, ProductSeed.SecondPaidTour.Name, ProductSeed.SecondPaidTour.Price, ProductSeed.SecondPaidTour.OwnerId);
    }

    public static ShoppingCart[] All => [SecondExplorersCartWithPaidTour, CartWithTwoTours];
}
