using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorersEmptyWallet = new(WellKnownUsers.Explorer);

    public static readonly Wallet WalletWithTwoDeposits = new(Guid.Parse("00000000-0000-0000-0002-000000000001"));

    public static readonly Wallet WalletWithOneDeposit = new(Guid.Parse("00000000-0000-0000-0002-000000000002"));

    static WalletSeed()
    {
        WalletWithTwoDeposits.Deposit(new Amount(100), WellKnownUsers.Administrator);
        WalletWithTwoDeposits.Deposit(new Amount(250), WellKnownUsers.Administrator);
        WalletWithOneDeposit.Deposit(new Amount(40), WellKnownUsers.Administrator);
    }

    public static Wallet[] All => [ExplorersEmptyWallet, WalletWithTwoDeposits, WalletWithOneDeposit];
}
