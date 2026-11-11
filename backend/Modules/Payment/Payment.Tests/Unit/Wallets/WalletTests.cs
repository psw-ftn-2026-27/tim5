using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static readonly Guid ReversingAdministrator = Guid.Parse("00000000-0000-0000-0003-000000000001");

    private static Wallet CreateWallet() => new(WellKnownUsers.Explorer);

    [Fact]
    public void Reversal_lowers_the_balance_by_the_deposit_and_marks_it_reversed_by_another_administrator()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);
        wallet.Deposit(new Amount(70), WellKnownUsers.Administrator);
        var deposit = wallet.Transactions[0];
        var now = deposit.OccurredAt.AddHours(1);

        wallet.Reverse(deposit.Id, ReversingAdministrator, now);

        wallet.Balance.Should().Be(70);
        deposit.IsReversed.Should().BeTrue();
        deposit.ReversedAt.Should().Be(now);
        deposit.ReversedByAdministratorId.Should().Be(ReversingAdministrator);
        deposit.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        deposit.Amount.Value.Should().Be(150);
    }

    [Fact]
    public void Reversal_exactly_24_hours_after_the_deposit_is_allowed()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);
        var deposit = wallet.Transactions[0];

        wallet.Reverse(deposit.Id, ReversingAdministrator, deposit.OccurredAt.AddHours(24));

        wallet.Balance.Should().Be(0);
        deposit.IsReversed.Should().BeTrue();
    }

    [Fact]
    public void Reversal_a_moment_after_24_hours_is_rejected_and_changes_nothing()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);
        var deposit = wallet.Transactions[0];

        var reverse = () => wallet.Reverse(deposit.Id, ReversingAdministrator, deposit.OccurredAt.AddHours(24).AddTicks(1));

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(150);
        deposit.IsReversed.Should().BeFalse();
        deposit.ReversedByAdministratorId.Should().BeNull();
    }

    [Fact]
    public void Second_reversal_of_the_same_deposit_is_rejected_and_keeps_the_balance_left_by_the_first()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);
        wallet.Deposit(new Amount(200), WellKnownUsers.Administrator);
        var deposit = wallet.Transactions[0];
        var firstReversalAt = deposit.OccurredAt.AddHours(1);
        wallet.Reverse(deposit.Id, ReversingAdministrator, firstReversalAt);

        var reverse = () => wallet.Reverse(deposit.Id, ReversingAdministrator, firstReversalAt.AddHours(1));

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(200);
        deposit.ReversedAt.Should().Be(firstReversalAt);
    }

    [Fact]
    public void Reversal_of_a_deposit_that_is_not_in_the_wallet_is_rejected_as_not_existing()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);

        var reverse = () => wallet.Reverse(Guid.NewGuid(), ReversingAdministrator, DateTime.UtcNow);

        reverse.Should().Throw<NotFoundException>();
        wallet.Balance.Should().Be(150);
    }

    [Fact]
    public void Deposit_increases_the_balance_by_the_amount_and_records_it_with_the_administrator()
    {
        var wallet = CreateWallet();

        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);

        wallet.Balance.Should().Be(150);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.Deposit &&
            transaction.Amount.Value == 150 &&
            transaction.AdministratorId == WellKnownUsers.Administrator);
    }

    [Fact]
    public void Second_deposit_adds_to_the_balance_left_by_the_first()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(150), WellKnownUsers.Administrator);

        wallet.Deposit(new Amount(70), WellKnownUsers.Administrator);

        wallet.Balance.Should().Be(220);
        wallet.Transactions.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void An_amount_that_is_not_positive_is_rejected(int value)
    {
        var create = () => new Amount(value);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void One_EC_is_the_smallest_accepted_deposit()
    {
        var wallet = CreateWallet();

        wallet.Deposit(new Amount(1), WellKnownUsers.Administrator);

        wallet.Balance.Should().Be(1);
    }
}
