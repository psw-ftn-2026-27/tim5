using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.GiftCardReading;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.GiftCardReading;

public class GiftCardReadingQueryTests : BaseIntegrationTest
{
    public GiftCardReadingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_tourist_sees_the_newest_sent_gift_card_on_the_first_page()
    {
        var expected = GiftCardSeed.All
            .Where(card => card.SenderId == WellKnownUsers.Explorer)
            .OrderByDescending(card => card.CreatedAt)
            .ThenByDescending(card => card.Id)
            .First();
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var page = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/sent?page=1&pageSize=1",
            JsonOptions);

        page!.TotalCount.Should().Be(2);
        page.Items.Select(card => card.Id).Should().Equal(expected.Id);
        page.Items[0].Id.Should().Be(GiftCardSeed.AnotherCardSentByExplorer.Id);
        page.Items[0].OtherPartyUsername.Should().Be("secondexplorer");
    }

    [Fact]
    public async Task A_tourist_sees_the_older_sent_gift_card_on_the_second_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var page = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/sent?page=2&pageSize=1",
            JsonOptions);

        page!.TotalCount.Should().Be(2);
        page.Items.Select(card => card.Id).Should().Equal(GiftCardSeed.CardSentByExplorer.Id);
    }

    [Fact]
    public async Task Received_gift_cards_are_paged_newest_first_with_status_and_sender_username()
    {
        var expected = GiftCardSeed.All
            .Where(card => card.RecipientId == WellKnownUsers.Explorer)
            .OrderByDescending(card => card.CreatedAt)
            .ThenByDescending(card => card.Id)
            .Take(2)
            .ToList();
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var firstPage = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/received?page=1&pageSize=2",
            JsonOptions);

        firstPage!.TotalCount.Should().Be(GiftCardSeed.All.Count(card => card.RecipientId == WellKnownUsers.Explorer));
        firstPage.Items.Select(card => card.Id).Should().Equal(expected.Select(card => card.Id));
        firstPage.Items.Should().OnlyContain(card => card.OtherPartyUsername == "secondexplorer");
        firstPage.Items.Single(card => card.Id == GiftCardSeed.RecentValidCardReceivedByExplorer.Id).Status
            .Should().Be(GiftCardStatus.Valid);
        firstPage.Items.Single(card => card.Id == GiftCardSeed.RedeemedCardReceivedByExplorer.Id).Status
            .Should().Be(GiftCardStatus.Redeemed);
    }

    [Fact]
    public async Task The_next_gift_card_page_contains_the_remaining_older_card()
    {
        var expected = GiftCardSeed.All
            .Where(card => card.RecipientId == WellKnownUsers.Explorer)
            .OrderByDescending(card => card.CreatedAt)
            .ThenByDescending(card => card.Id)
            .Skip(2)
            .Take(2)
            .ToList();
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var page = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/received?page=2&pageSize=2",
            JsonOptions);

        page!.TotalCount.Should().Be(GiftCardSeed.All.Count(card => card.RecipientId == WellKnownUsers.Explorer));
        page.Items.Select(card => card.Id).Should().Equal(expected.Select(card => card.Id));
        page.Items.Single().Status.Should().Be(GiftCardStatus.Expired);
    }

    [Fact]
    public async Task Gift_card_list_paging_clamps_page_and_page_size_to_valid_values()
    {
        var expected = GiftCardSeed.All
            .Where(card => card.RecipientId == WellKnownUsers.Explorer)
            .OrderByDescending(card => card.CreatedAt)
            .ThenByDescending(card => card.Id)
            .First();
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var page = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/received?page=0&pageSize=0",
            JsonOptions);

        page!.TotalCount.Should().Be(GiftCardSeed.All.Count(card => card.RecipientId == WellKnownUsers.Explorer));
        page.Items.Select(card => card.Id).Should().Equal(expected.Id);
    }

    [Fact]
    public async Task An_anonymous_user_cannot_read_gift_cards()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/payment/gift-cards/received");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_administrator_cannot_read_gift_cards_as_a_tourist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/gift-cards/received");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
