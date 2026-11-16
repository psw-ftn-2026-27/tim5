using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly ShoppingCart SecondExplorersCartWithPaidTour = new(WellKnownUsers.SecondExplorer);

    public static readonly ShoppingCart CartWithTwoTours = new(Guid.Parse("00000000-0000-0000-0005-000000000001"));

    public static readonly ShoppingCart BuyersCartWithEnoughEC = new(WalletSeed.BuyersWalletWithEnoughEC.TouristId);

    public static readonly ShoppingCart BuyersCartShortOfEC = new(WalletSeed.BuyersWalletShortOfEC.TouristId);

    static ShoppingCartSeed()
    {
        SecondExplorersCartWithPaidTour.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId, false);
        CartWithTwoTours.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId, false);
        CartWithTwoTours.AddItem(
            ProductSeed.SecondPaidTour.EntityId, ProductSeed.SecondPaidTour.Name, ProductSeed.SecondPaidTour.Price, ProductSeed.SecondPaidTour.OwnerId, false);
        BuyersCartWithEnoughEC.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId, false);
        BuyersCartWithEnoughEC.AddItem(
            ProductSeed.SecondPaidTour.EntityId, ProductSeed.SecondPaidTour.Name, ProductSeed.SecondPaidTour.Price, ProductSeed.SecondPaidTour.OwnerId, false);
        BuyersCartShortOfEC.AddItem(
            ProductSeed.PaidTour.EntityId, ProductSeed.PaidTour.Name, ProductSeed.PaidTour.Price, ProductSeed.PaidTour.OwnerId, false);
        BuyersCartShortOfEC.AddItem(
            ProductSeed.SecondPaidTour.EntityId, ProductSeed.SecondPaidTour.Name, ProductSeed.SecondPaidTour.Price, ProductSeed.SecondPaidTour.OwnerId, false);
    }

    public static ShoppingCart[] All => [SecondExplorersCartWithPaidTour, CartWithTwoTours, BuyersCartWithEnoughEC, BuyersCartShortOfEC];
}
