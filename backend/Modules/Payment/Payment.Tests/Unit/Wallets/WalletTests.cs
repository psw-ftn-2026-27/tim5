using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Wallets;

public class WalletTests
{
    private static readonly Guid ReversingAdministrator = Guid.Parse("00000000-0000-0000-0003-000000000001");

    private static readonly DateTime PaidAt = new(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc);

    private static Wallet CreateWallet() => new(WellKnownUsers.Explorer);

    private static List<PurchaseItem> CreateTwoTours() =>
    [
        new(Guid.Parse("00000000-0000-0000-0006-000000000011"), "Petrovaradinska tvrdjava", new Amount(150)),
        new(Guid.Parse("00000000-0000-0000-0006-000000000012"), "Novosadska sinagoga", new Amount(200))
    ];

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

    [Fact]
    public void Purchase_of_two_tours_lowers_the_balance_by_their_sum_and_records_a_purchase_with_the_name_and_price_of_each()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(500), WellKnownUsers.Administrator);
        var tours = CreateTwoTours();

        wallet.Purchase(tours, PaidAt);

        wallet.Balance.Should().Be(150);
        var purchases = wallet.Transactions.Where(transaction => transaction.Type == TransactionType.Purchase).ToList();
        purchases.Select(purchase => new { purchase.EntityId, purchase.Name, Amount = purchase.Amount.Value, purchase.OccurredAt })
            .Should().BeEquivalentTo(tours.Select(tour => new { EntityId = (Guid?)tour.EntityId, Name = (string?)tour.Name, Amount = tour.Price.Value, OccurredAt = PaidAt }));
    }

    [Fact]
    public void Purchase_with_a_balance_exactly_equal_to_the_total_is_allowed_and_leaves_zero()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(350), WellKnownUsers.Administrator);

        wallet.Purchase(CreateTwoTours(), PaidAt);

        wallet.Balance.Should().Be(0);
    }

    [Fact]
    public void Purchase_with_a_balance_one_EC_short_of_the_total_is_rejected_and_changes_nothing()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(349), WellKnownUsers.Administrator);

        var purchase = () => wallet.Purchase(CreateTwoTours(), PaidAt);

        purchase.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(349);
        wallet.Transactions.Should().ContainSingle();
    }

    [Fact]
    public void Gift_card_purchase_lowers_the_balance_and_records_its_reference_without_product_details()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(500), WellKnownUsers.Administrator);
        var giftCardId = Guid.NewGuid();

        wallet.PurchaseGiftCard(giftCardId, new Amount(100), PaidAt);

        wallet.Balance.Should().Be(400);
        wallet.Transactions.Should().ContainSingle(transaction =>
            transaction.Type == TransactionType.GiftCardPurchase
            && transaction.Amount.Value == 100
            && transaction.GiftCardId == giftCardId
            && transaction.EntityId == null
            && transaction.Name == null
            && transaction.OccurredAt == PaidAt);
    }

    [Fact]
    public void Gift_card_purchase_without_enough_EC_is_rejected_without_changing_the_wallet()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(50), WellKnownUsers.Administrator);

        var purchase = () => wallet.PurchaseGiftCard(Guid.NewGuid(), new Amount(100), PaidAt);

        purchase.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(50);
        wallet.Transactions.Should().ContainSingle();
    }

    [Fact]
    public void Reversal_of_a_deposit_that_a_purchase_has_partly_spent_is_rejected_and_keeps_the_balance()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(400), WellKnownUsers.Administrator);
        var deposit = wallet.Transactions[0];
        wallet.Purchase(CreateTwoTours(), PaidAt);

        var reverse = () => wallet.Reverse(deposit.Id, ReversingAdministrator, deposit.OccurredAt.AddHours(1));

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(50);
        deposit.IsReversed.Should().BeFalse();
    }

    [Fact]
    public void Reversal_of_a_purchase_is_rejected_and_keeps_the_balance()
    {
        var wallet = CreateWallet();
        wallet.Deposit(new Amount(500), WellKnownUsers.Administrator);
        wallet.Purchase(CreateTwoTours(), PaidAt);
        var purchase = wallet.Transactions.First(transaction => transaction.Type == TransactionType.Purchase);

        var reverse = () => wallet.Reverse(purchase.Id, ReversingAdministrator, PaidAt.AddHours(1));

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(150);
        purchase.IsReversed.Should().BeFalse();
    }
}
