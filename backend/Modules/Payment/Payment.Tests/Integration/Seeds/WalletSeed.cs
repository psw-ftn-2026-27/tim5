using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersEmptyWallet = new(WellKnownUsers.Explorer);

    public static readonly Wallet WalletWithTwoDeposits = new(Guid.Parse("00000000-0000-0000-0002-000000000001"));

    public static readonly Wallet WalletWithOneDeposit = new(Guid.Parse("00000000-0000-0000-0002-000000000002"));

    public static readonly Wallet WalletWithOneReversedDeposit = new(Guid.Parse("00000000-0000-0000-0002-000000000003"));

    public static readonly Wallet BuyersWalletWithEnoughEC = new(Guid.Parse("00000000-0000-0000-0002-000000000004"));

    public static readonly Wallet BuyersWalletShortOfEC = new(Guid.Parse("00000000-0000-0000-0002-000000000005"));

    public static readonly Wallet WalletWithTwoPurchases = new(Guid.Parse("00000000-0000-0000-0002-000000000006"));

    public static readonly Wallet AnotherTouristsWalletWithOnePurchase = new(Guid.Parse("00000000-0000-0000-0002-000000000007"));

    static WalletSeed()
    {
        WalletWithTwoDeposits.Deposit(new Amount(100), WellKnownUsers.Administrator);
        WalletWithTwoDeposits.Deposit(new Amount(250), WellKnownUsers.Administrator);
        WalletWithOneDeposit.Deposit(new Amount(40), WellKnownUsers.Administrator);
        WalletWithOneReversedDeposit.Deposit(new Amount(100), WellKnownUsers.Administrator);
        WalletWithOneReversedDeposit.Deposit(new Amount(60), WellKnownUsers.Administrator);
        WalletWithOneReversedDeposit.Reverse(WalletWithOneReversedDeposit.Transactions[1].Id, WellKnownUsers.Administrator, DateTime.UtcNow);
        BuyersWalletWithEnoughEC.Deposit(new Amount(500), WellKnownUsers.Administrator);
        BuyersWalletShortOfEC.Deposit(new Amount(100), WellKnownUsers.Administrator);
        WalletWithTwoPurchases.Deposit(new Amount(500), WellKnownUsers.Administrator);
        WalletWithTwoPurchases.Purchase(
            [new PurchaseItem(ProductSeed.PaidTour.EntityId, "Petrovaradin Fortress", new Amount(150))],
            new DateTime(2026, 11, 10, 9, 0, 0, DateTimeKind.Utc));
        WalletWithTwoPurchases.Purchase(
            [new PurchaseItem(ProductSeed.SecondPaidTour.EntityId, ProductSeed.SecondPaidTour.Name, new Amount(200))],
            new DateTime(2026, 11, 12, 9, 0, 0, DateTimeKind.Utc));
        AnotherTouristsWalletWithOnePurchase.Deposit(new Amount(300), WellKnownUsers.Administrator);
        AnotherTouristsWalletWithOnePurchase.Purchase(
            [new PurchaseItem(ProductSeed.PaidTour.EntityId, "Petrovaradin Fortress", new Amount(150))],
            new DateTime(2026, 11, 11, 9, 0, 0, DateTimeKind.Utc));
    }

    public static Wallet[] All =>
        [ExplorersEmptyWallet, WalletWithTwoDeposits, WalletWithOneDeposit, WalletWithOneReversedDeposit, BuyersWalletWithEnoughEC, BuyersWalletShortOfEC,
            WalletWithTwoPurchases, AnotherTouristsWalletWithOnePurchase];
}
