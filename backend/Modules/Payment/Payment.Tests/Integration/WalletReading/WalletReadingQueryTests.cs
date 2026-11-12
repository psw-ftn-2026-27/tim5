using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.WalletReading;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletReading;

public class WalletReadingQueryTests : BaseIntegrationTest
{
    public WalletReadingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_without_a_wallet_sees_zero_balance_and_no_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Balance.Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Tourist_with_an_empty_wallet_sees_zero_balance_and_no_transactions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Balance.Should().Be(WalletSeed.ExplorersEmptyWallet.Balance);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public async Task Tourist_sees_both_deposits_with_type_amount_and_time_from_newest_to_oldest()
    {
        var seeded = WalletSeed.WalletWithTwoDeposits;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");
        var expected = seeded.Transactions
            .Select(transaction => new { transaction.Type, Amount = transaction.Amount.Value })
            .ToList();

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Transactions
            .Select(transaction => new { transaction.Type, transaction.Amount })
            .Should().BeEquivalentTo(expected);
        wallet.Transactions.Should().BeInDescendingOrder(transaction => transaction.OccurredAt);
        wallet.Transactions.Should().OnlyContain(transaction => transaction.OccurredAt != default);
    }

    [Fact]
    public async Task Tourist_sees_only_their_own_wallet_while_another_tourist_has_deposits()
    {
        var seeded = WalletSeed.WalletWithOneDeposit;
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Balance.Should().Be(seeded.Balance);
        wallet.Transactions.Select(transaction => transaction.Amount)
            .Should().Equal(seeded.Transactions.Select(transaction => transaction.Amount.Value));
    }

    [Fact]
    public async Task Tourist_sees_a_reversed_deposit_with_its_original_amount_and_time_marked_reversed_with_the_reversal_time()
    {
        var seeded = WalletSeed.WalletWithOneReversedDeposit;
        var reversed = seeded.Transactions.Single(transaction => transaction.IsReversed);
        var client = Factory.CreateClientFor(seeded.TouristId, "explorer");

        var wallet = await client.GetFromJsonAsync<WalletDto>("/api/payment/wallets/mine", JsonOptions);

        wallet!.Balance.Should().Be(seeded.Balance);
        var shown = wallet.Transactions.Single(transaction => transaction.IsReversed);
        shown.Amount.Should().Be(reversed.Amount.Value);
        shown.OccurredAt.Should().BeCloseTo(reversed.OccurredAt, TimeSpan.FromMilliseconds(1));
        shown.ReversedAt.Should().BeCloseTo(reversed.ReversedAt!.Value, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_a_wallet()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/wallets/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_a_wallet_as_a_tourist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/wallets/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
