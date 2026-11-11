using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Depositing;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Depositing;

public class DepositingQueryTests : BaseIntegrationTest
{
    public DepositingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Search_shows_found_users_without_a_wallet_with_zero_and_found_users_with_a_wallet_with_their_balance()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tourists = await client.GetFromJsonAsync<List<TouristBalanceDto>>("/api/payment/tourists?username=explorer", JsonOptions);

        tourists.Should().ContainSingle(tourist =>
            tourist.TouristId == WellKnownUsers.SecondExplorer && tourist.Balance == 0);
        tourists.Should().ContainSingle(tourist =>
            tourist.TouristId == WalletSeed.ExplorersEmptyWallet.TouristId
            && tourist.Balance == WalletSeed.ExplorersEmptyWallet.Balance);
        tourists.Should().ContainSingle(tourist =>
            tourist.TouristId == WalletSeed.WalletWithOneDeposit.TouristId
            && tourist.Balance == WalletSeed.WalletWithOneDeposit.Balance);
    }
}
