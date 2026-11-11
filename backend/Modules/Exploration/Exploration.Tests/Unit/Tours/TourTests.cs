using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);
    private static readonly Price FreePrice = new(true, 0);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"], new GroupSize(2, 6));

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.Publish(FreePrice);
        return tour;
    }

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"], new GroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, [], new GroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_a_recommended_group_size()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"], null!);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);

        tour.Publish(FreePrice);

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_without_a_price_cannot_be_published_and_stays_a_draft()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish(null!);

        publishing.Should().Throw<DomainException>();
        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        tour.Publish(FreePrice);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_is_archived_with_its_archiving_time()
    {
        var tour = CreatePublishedTour();

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_that_is_not_published_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_is_reactivated_without_an_archiving_time_and_keeps_its_publishing_time()
    {
        var tour = CreatePublishedTour();
        var publishedAt = tour.PublishedAt;
        tour.Archive();

        tour.Reactivate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.ArchivedAt.Should().BeNull();
        tour.PublishedAt.Should().Be(publishedAt);
    }

    [Fact]
    public void Tour_that_is_not_archived_cannot_be_reactivated()
    {
        var tour = CreateTour(LongDescription);

        var reactivation = () => tour.Reactivate();

        reactivation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published_again()
    {
        var tour = CreatePublishedTour();
        tour.Archive();

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }
}
