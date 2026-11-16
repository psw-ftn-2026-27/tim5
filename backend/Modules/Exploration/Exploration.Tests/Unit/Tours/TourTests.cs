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
        AddKeyPoint(tour);
        AddKeyPoint(tour);
        tour.Publish(FreePrice);
        return tour;
    }

    private static Tour CreateMeasuredDraftWithThreeKeyPoints()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPoint(tour);
        AddKeyPoint(tour);
        AddKeyPoint(tour);
        tour.RecordLength(3.5m);
        return tour;
    }

    private static void AddKeyPoint(Tour tour) =>
        tour.AddKeyPoint("Tačka", "Opis tačke.", Guid.NewGuid(), null, false, new Location(45.25, 19.85));

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
        AddKeyPoint(tour);
        AddKeyPoint(tour);

        tour.Publish(FreePrice);

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Tour_without_a_price_cannot_be_published_and_stays_a_draft()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPoint(tour);
        AddKeyPoint(tour);

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
        AddKeyPoint(tour);
        AddKeyPoint(tour);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPoint(tour);
        AddKeyPoint(tour);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreatePublishedTour();

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

    [Theory]
    [InlineData("", "Opis tačke.", true)]
    [InlineData("   ", "Opis tačke.", true)]
    [InlineData("Tačka", "", true)]
    [InlineData("Tačka", "   ", true)]
    [InlineData("Tačka", "Opis tačke.", false)]
    public void Key_point_without_a_name_description_or_image_cannot_be_added(string name, string description, bool hasImage)
    {
        var tour = CreateTour(LongDescription);
        var imageId = hasImage ? Guid.NewGuid() : Guid.Empty;

        var addition = () => tour.AddKeyPoint(name, description, imageId, null, false, new Location(45.25, 19.85));

        addition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Key_point_cannot_be_added_to_a_published_tour()
    {
        var tour = CreatePublishedTour();

        var addition = () => tour.AddKeyPoint("Nova tačka", "Opis tačke.", Guid.NewGuid(), null, false, new Location(45.25, 19.85));

        addition.Should().Throw<DomainException>();
        tour.KeyPoints.Should().HaveCount(2);
    }

    [Fact]
    public void Key_point_of_a_published_tour_cannot_be_changed()
    {
        var tour = CreatePublishedTour();
        var keyPoint = tour.KeyPoints[0];

        var change = () => tour.ChangeKeyPoint(keyPoint.Id, "Novi naziv", "Novi opis.", Guid.NewGuid(), null, true, new Location(45.26, 19.86));

        change.Should().Throw<DomainException>();
        keyPoint.Name.Should().Be("Tačka");
    }

    [Fact]
    public void Key_point_of_a_published_tour_cannot_be_removed()
    {
        var tour = CreatePublishedTour();
        var keyPointId = tour.KeyPoints[0].Id;

        var removal = () => tour.RemoveKeyPoint(keyPointId);

        removal.Should().Throw<DomainException>();
        tour.KeyPoints.Should().Contain(keyPoint => keyPoint.Id == keyPointId);
    }

    [Fact]
    public void Remaining_key_points_keep_their_order_without_gaps_after_a_removal()
    {
        var tour = CreateMeasuredDraftWithThreeKeyPoints();
        var first = tour.KeyPoints[0];
        var middle = tour.KeyPoints[1];
        var last = tour.KeyPoints[2];

        tour.RemoveKeyPoint(middle.Id);

        tour.KeyPoints.Should().HaveCount(2);
        first.Order.Should().Be(1);
        last.Order.Should().Be(2);
    }

    [Fact]
    public void Adding_a_key_point_clears_the_tour_length()
    {
        var tour = CreateMeasuredDraftWithThreeKeyPoints();

        tour.AddKeyPoint("Nova tačka", "Opis tačke.", Guid.NewGuid(), null, false, new Location(45.25, 19.85));

        tour.LengthInKm.Should().BeNull();
    }

    [Fact]
    public void Changing_a_key_point_clears_the_tour_length()
    {
        var tour = CreateMeasuredDraftWithThreeKeyPoints();

        tour.ChangeKeyPoint(tour.KeyPoints[0].Id, "Novi naziv", "Novi opis.", Guid.NewGuid(), "Tajna", true, new Location(45.26, 19.86));

        tour.LengthInKm.Should().BeNull();
    }

    [Fact]
    public void Removing_a_key_point_clears_the_tour_length()
    {
        var tour = CreateMeasuredDraftWithThreeKeyPoints();

        tour.RemoveKeyPoint(tour.KeyPoints[0].Id);

        tour.LengthInKm.Should().BeNull();
    }

    [Fact]
    public void Tour_with_one_key_point_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 60);
        AddKeyPoint(tour);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
        tour.Status.Should().Be(TourStatus.Draft);
    }

    [Fact]
    public void Tour_with_two_key_points_and_an_unknown_length_is_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 60);
        AddKeyPoint(tour);
        AddKeyPoint(tour);

        tour.Publish(FreePrice);

        tour.Status.Should().Be(TourStatus.Published);
        tour.LengthInKm.Should().BeNull();
    }
}
