using System.Net.Http.Json;
using FluentAssertions;
using Identity.Tests.Integration.Seeds;
using Payment.Application.Reversing;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Reversing;

public class ReversingQueryTests : BaseIntegrationTest
{
    public ReversingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Deposits_of_a_tourist_come_with_the_username_the_balance_and_the_status_of_each_deposit()
    {
        var seeded = WalletSeed.WalletWithOneDeposit;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expected = seeded.Transactions
            .Select(transaction => new
            {
                transaction.Id,
                Amount = transaction.Amount.Value,
                transaction.IsReversed,
                transaction.ReversedAt
            })
            .ToList();

        var deposits = await client.GetFromJsonAsync<TouristDepositsDto>($"/api/payment/tourists/{seeded.TouristId}/deposits", JsonOptions);

        deposits!.Username.Should().Be(UserSeed.ExplorerWithDeposit.UserName);
        deposits.Balance.Should().Be(seeded.Balance);
        deposits.Deposits
            .Select(deposit => new { deposit.Id, deposit.Amount, deposit.IsReversed, deposit.ReversedAt })
            .Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Tourist_without_a_wallet_has_zero_balance_and_no_deposits()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var deposits = await client.GetFromJsonAsync<TouristDepositsDto>($"/api/payment/tourists/{WellKnownUsers.SecondExplorer}/deposits", JsonOptions);

        deposits!.Username.Should().Be(UserSeed.SecondExplorer.UserName);
        deposits.Balance.Should().Be(0);
        deposits.Deposits.Should().BeEmpty();
    }
}
