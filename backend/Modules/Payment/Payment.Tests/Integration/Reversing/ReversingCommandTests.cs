using System.Net;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Reversing;

public class ReversingCommandTests : BaseIntegrationTest
{
    public ReversingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Reversal_lowers_the_balance_by_the_deposit_and_records_the_time_and_the_reversing_administrator()
    {
        var seeded = WalletSeed.WalletWithTwoDeposits;
        var deposit = seeded.Transactions[0];
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/tourists/{seeded.TouristId}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Single(wallet => wallet.Id == seeded.Id);
        stored.Balance.Should().Be(seeded.Balance - deposit.Amount.Value);
        var storedDeposit = stored.Transactions.Single(transaction => transaction.Id == deposit.Id);
        storedDeposit.ReversedAt.Should().NotBeNull();
        storedDeposit.ReversedByAdministratorId.Should().Be(WellKnownUsers.Administrator);
        storedDeposit.Amount.Value.Should().Be(deposit.Amount.Value);
    }

    [Fact]
    public async Task Reversal_of_an_already_reversed_deposit_is_rejected_and_leaves_the_wallet_unchanged()
    {
        var seeded = WalletSeed.WalletWithOneReversedDeposit;
        var deposit = seeded.Transactions.Single(transaction => transaction.IsReversed);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/tourists/{seeded.TouristId}/deposits/{deposit.Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Single(wallet => wallet.Id == seeded.Id);
        stored.Balance.Should().Be(seeded.Balance);
        stored.Transactions.Single(transaction => transaction.Id == deposit.Id).ReversedAt
            .Should().BeCloseTo(deposit.ReversedAt!.Value, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Reversal_for_a_tourist_without_a_wallet_is_rejected_and_creates_no_wallet()
    {
        var touristId = WellKnownUsers.SecondExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/payment/tourists/{touristId}/deposits/{Guid.NewGuid()}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Should().NotContain(wallet => wallet.TouristId == touristId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reverse_a_deposit()
    {
        var seeded = WalletSeed.WalletWithOneDeposit;
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/payment/tourists/{seeded.TouristId}/deposits/{seeded.Transactions[0].Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tourist_cannot_reverse_a_deposit()
    {
        var seeded = WalletSeed.WalletWithOneDeposit;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var response = await client.PostAsync($"/api/payment/tourists/{seeded.TouristId}/deposits/{seeded.Transactions[0].Id}/reversal", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
