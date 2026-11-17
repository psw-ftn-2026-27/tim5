using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    public static readonly Guid GiftCardBuyerId = Guid.Parse("00000000-0000-0000-0000-000000000099");
    public static readonly Guid MissingIdentityRecipientId = Guid.Parse("00000000-0000-0000-0000-000000000098");

    public static readonly GiftCard RecentValidCardReceivedByExplorer = new(
        WellKnownUsers.SecondExplorer,
        WellKnownUsers.Explorer,
        new Amount(100),
        "A recent gift",
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard ExpiredCardReceivedByExplorer = new(
        WellKnownUsers.SecondExplorer,
        WellKnownUsers.Explorer,
        new Amount(150),
        "An expired gift",
        new DateTime(2022, 1, 1, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard RedeemedCardReceivedByExplorer = new(
        WellKnownUsers.SecondExplorer,
        WellKnownUsers.Explorer,
        new Amount(200),
        "A redeemed gift",
        new DateTime(2025, 5, 1, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard CardSentByExplorer = new(
        WellKnownUsers.Explorer,
        WellKnownUsers.SecondExplorer,
        new Amount(75),
        "A sent gift",
        new DateTime(2026, 11, 10, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard AnotherCardSentByExplorer = new(
        WellKnownUsers.Explorer,
        WellKnownUsers.SecondExplorer,
        new Amount(90),
        "A newer sent gift",
        new DateTime(2026, 11, 12, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard GiftCardBuyerSentToSecondExplorer = new(
        GiftCardBuyerId,
        WellKnownUsers.SecondExplorer,
        new Amount(80),
        "A gift from the test buyer",
        new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard GiftCardSentToUserMissingFromIdentity = new(
        GiftCardBuyerId,
        MissingIdentityRecipientId,
        new Amount(50),
        "A gift for an unavailable user",
        new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc));

    static GiftCardSeed()
    {
        RedeemedCardReceivedByExplorer.Redeem(
            WellKnownUsers.Explorer,
            new DateTime(2025, 5, 2, 10, 0, 0, DateTimeKind.Utc));
    }

    public static GiftCard[] All =>
        [RecentValidCardReceivedByExplorer, ExpiredCardReceivedByExplorer, RedeemedCardReceivedByExplorer, CardSentByExplorer, AnotherCardSentByExplorer, GiftCardBuyerSentToSecondExplorer, GiftCardSentToUserMissingFromIdentity];
}
