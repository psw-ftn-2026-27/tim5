using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.Depositing;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.Depositing;

public class DepositingCommandTests : BaseIntegrationTest
{
    public DepositingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Deposit_to_a_tourist_with_a_wallet_increases_the_balance_and_records_the_transaction()
    {
        var seeded = WalletSeed.ExplorersEmptyWallet;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{seeded.TouristId}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Single(wallet => wallet.Id == seeded.Id);
        stored.Balance.Should().Be(seeded.Balance + 100);
        stored.Transactions.Should().HaveCount(seeded.Transactions.Count + 1);
        stored.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Deposit
            && transaction.Amount.Value == 100
            && transaction.AdministratorId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Deposit_to_a_tourist_without_a_wallet_creates_the_wallet_with_the_deposited_amount()
    {
        var touristId = WellKnownUsers.SecondExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{touristId}/deposits", new DepositDto(70));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore + 1);
        var stored = assertContext.Wallets.Single(wallet => wallet.TouristId == touristId);
        stored.Balance.Should().Be(70);
        stored.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Deposit
            && transaction.Amount.Value == 70
            && transaction.AdministratorId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Deposit_to_a_user_who_does_not_exist_is_rejected_and_creates_no_wallet()
    {
        var unknownUserId = Guid.NewGuid();
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var walletCountBefore = arrangeContext.Wallets.Count();

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{unknownUserId}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Wallets.Count().Should().Be(walletCountBefore);
        assertContext.Wallets.Should().NotContain(wallet => wallet.TouristId == unknownUserId);
    }

    [Fact]
    public async Task Deposit_of_zero_is_rejected_and_leaves_the_wallet_unchanged()
    {
        var seeded = WalletSeed.WalletWithOneDeposit;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{seeded.TouristId}/deposits", new DepositDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        var stored = assertContext.Wallets.Single(wallet => wallet.Id == seeded.Id);
        stored.Balance.Should().Be(seeded.Balance);
        stored.Transactions.Should().HaveSameCount(seeded.Transactions);
    }

    [Fact]
    public async Task Anonymous_user_cannot_deposit()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{WellKnownUsers.Explorer}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Tourist_cannot_deposit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync($"/api/payment/tourists/{WellKnownUsers.Explorer}/deposits", new DepositDto(100));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
