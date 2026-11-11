using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static Wallet CreateWallet() => new(WellKnownUsers.Explorer);

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
