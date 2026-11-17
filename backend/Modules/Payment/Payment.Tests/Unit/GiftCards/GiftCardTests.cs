using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardTests
{
    private static readonly DateTime CreatedAt = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    private static GiftCard CreateGiftCard() => new(
        WellKnownUsers.Explorer,
        WellKnownUsers.SecondExplorer,
        new Amount(100),
        "Enjoy the tour",
        CreatedAt);

    [Fact]
    public void A_gift_card_has_a_three_year_validity_period()
    {
        var giftCard = CreateGiftCard();

        giftCard.CreatedAt.Should().Be(CreatedAt);
        giftCard.ExpiresAt.Should().Be(CreatedAt.AddYears(3));
        giftCard.RedeemedAt.Should().BeNull();
        giftCard.GetStatus(CreatedAt).Should().Be(GiftCardStatus.Valid);
    }

    [Fact]
    public void A_gift_card_expires_when_the_expiry_time_is_reached()
    {
        var giftCard = CreateGiftCard();

        giftCard.GetStatus(giftCard.ExpiresAt).Should().Be(GiftCardStatus.Expired);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void Gift_card_amount_at_either_allowed_boundary_is_accepted(int amount)
    {
        var giftCard = new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.SecondExplorer,
            new Amount(amount),
            "Enjoy the tour",
            CreatedAt);

        giftCard.Amount.Value.Should().Be(amount);
    }

    [Fact]
    public void Redeeming_a_gift_card_records_the_time_and_makes_it_unavailable()
    {
        var giftCard = CreateGiftCard();
        var redeemedAt = CreatedAt.AddDays(1);

        giftCard.Redeem(WellKnownUsers.SecondExplorer, redeemedAt);

        giftCard.RedeemedAt.Should().Be(redeemedAt);
        giftCard.GetStatus(redeemedAt).Should().Be(GiftCardStatus.Redeemed);
        giftCard.GetStatus(giftCard.ExpiresAt).Should().Be(GiftCardStatus.Redeemed);
    }

    [Fact]
    public void A_gift_card_cannot_be_redeemed_by_another_user()
    {
        var giftCard = CreateGiftCard();

        var redeem = () => giftCard.Redeem(WellKnownUsers.Explorer, CreatedAt.AddDays(1));

        redeem.Should().Throw<DomainException>();
        giftCard.RedeemedAt.Should().BeNull();
    }

    [Fact]
    public void An_expired_gift_card_cannot_be_redeemed()
    {
        var giftCard = CreateGiftCard();

        var redeem = () => giftCard.Redeem(WellKnownUsers.SecondExplorer, giftCard.ExpiresAt);

        redeem.Should().Throw<DomainException>();
        giftCard.RedeemedAt.Should().BeNull();
    }

    [Fact]
    public void A_redeemed_gift_card_cannot_be_redeemed_again()
    {
        var giftCard = CreateGiftCard();
        giftCard.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddDays(1));

        var redeem = () => giftCard.Redeem(WellKnownUsers.SecondExplorer, CreatedAt.AddDays(2));

        redeem.Should().Throw<DomainException>();
        giftCard.RedeemedAt.Should().Be(CreatedAt.AddDays(1));
    }

    [Fact]
    public void A_gift_card_cannot_be_sent_to_its_sender()
    {
        var create = () => new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.Explorer,
            new Amount(100),
            "Enjoy the tour",
            CreatedAt);

        create.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    public void A_gift_card_amount_outside_the_allowed_range_is_rejected(int amount)
    {
        var create = () => new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.SecondExplorer,
            new Amount(amount),
            "Enjoy the tour",
            CreatedAt);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_gift_card_message_over_200_characters_is_rejected()
    {
        var create = () => new GiftCard(
            WellKnownUsers.Explorer,
            WellKnownUsers.SecondExplorer,
            new Amount(100),
            new string('a', 201),
            CreatedAt);

        create.Should().Throw<DomainException>();
    }
}
