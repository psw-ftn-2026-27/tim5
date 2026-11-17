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

    private static readonly Guid CheckoutBuyerId = Guid.Parse("00000000-0000-0000-0002-000000000004");
    private static readonly Guid ArchivedCheckoutBuyerId = Guid.Parse("00000000-0000-0000-0002-000000000008");

    public static readonly GiftCard CheckoutCardForBuyer = new(
        WellKnownUsers.SecondExplorer,
        CheckoutBuyerId,
        new Amount(100),
        "Checkout benefit",
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard CheckoutCardForAnotherBuyer = new(
        WellKnownUsers.SecondExplorer,
        GiftCardBuyerId,
        new Amount(100),
        "Belongs to another buyer",
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard OversizedCheckoutCardForBuyer = new(
        WellKnownUsers.SecondExplorer,
        CheckoutBuyerId,
        new Amount(300),
        "Covers the full cart",
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard ExpiredCheckoutCardForBuyer = new(
        WellKnownUsers.SecondExplorer,
        CheckoutBuyerId,
        new Amount(100),
        "Expired checkout card",
        new DateTime(2020, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    public static readonly GiftCard CheckoutCardForArchivedCartBuyer = new(
        WellKnownUsers.SecondExplorer,
        ArchivedCheckoutBuyerId,
        new Amount(100),
        "Checkout card for archived tour test",
        new DateTime(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc));

    static GiftCardSeed()
    {
        RedeemedCardReceivedByExplorer.Redeem(
            WellKnownUsers.Explorer,
            new DateTime(2025, 5, 2, 10, 0, 0, DateTimeKind.Utc));
    }

    public static GiftCard[] All =>
        [RecentValidCardReceivedByExplorer, ExpiredCardReceivedByExplorer, RedeemedCardReceivedByExplorer, CardSentByExplorer, AnotherCardSentByExplorer, GiftCardBuyerSentToSecondExplorer, GiftCardSentToUserMissingFromIdentity, CheckoutCardForBuyer, CheckoutCardForAnotherBuyer, OversizedCheckoutCardForBuyer, ExpiredCheckoutCardForBuyer, CheckoutCardForArchivedCartBuyer];
}
