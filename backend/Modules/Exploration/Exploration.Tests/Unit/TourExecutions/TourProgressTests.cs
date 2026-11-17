using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourProgressTests
{
    private static readonly DateTime StartedAt = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CheckedAt = new(2026, 11, 17, 9, 30, 0, DateTimeKind.Utc);
    private static readonly Location ClockTower = new(45.2517, 19.8620);
    private static readonly Location Galleries = new(45.2530, 19.8636);
    private static readonly Location UpperPlateau = new(45.2522, 19.8660);

    private static Tour CreatePublishedTourWithThreeKeyPoints()
    {
        var tour = new Tour(WellKnownUsers.Explorer, "Tajne Petrovaradina", new string('o', 100), TourDifficulty.Easy, ["tvrđava"], new GroupSize(1, 10));
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.AddKeyPoint("Sahat kula", "Opis tačke.", Guid.NewGuid(), "Prva tajna.", true, ClockTower);
        tour.AddKeyPoint("Ulaz u galerije", "Opis tačke.", Guid.NewGuid(), "Druga tajna.", false, Galleries);
        tour.AddKeyPoint("Gornji plato", "Opis tačke.", Guid.NewGuid(), "Treća tajna.", false, UpperPlateau);
        tour.Publish(new Price(true, 0));
        return tour;
    }

    private static TourExecution CreateActiveExecution(Tour tour) =>
        new(WellKnownUsers.SecondExplorer, tour.Id, ClockTower, StartedAt);

    [Fact]
    public void Explorer_near_the_next_key_point_completes_it_with_the_time_it_was_reached()
    {
        var tour = CreatePublishedTourWithThreeKeyPoints();
        var execution = CreateActiveExecution(tour);
        var elevenMetersFromClockTower = new Location(45.2518, 19.8620);

        TourProgress.CheckPosition(execution, tour, elevenMetersFromClockTower, CheckedAt);

        execution.CompletedKeyPoints.Should().Equal(new CompletedKeyPoint(tour.KeyPoints[0].Id, CheckedAt));
        execution.Status.Should().Be(TourExecutionStatus.Active);
    }

    [Fact]
    public void Explorer_far_from_the_next_key_point_does_not_complete_it_but_the_activity_is_recorded()
    {
        var tour = CreatePublishedTourWithThreeKeyPoints();
        var execution = CreateActiveExecution(tour);
        var hundredMetersFromClockTower = new Location(45.2526, 19.8620);

        TourProgress.CheckPosition(execution, tour, hundredMetersFromClockTower, CheckedAt);

        execution.CompletedKeyPoints.Should().BeEmpty();
        execution.LastActivity.Should().Be(CheckedAt);
    }

    [Fact]
    public void Key_point_that_is_not_next_is_not_completed_even_when_the_explorer_stands_right_next_to_it()
    {
        var tour = CreatePublishedTourWithThreeKeyPoints();
        var execution = CreateActiveExecution(tour);

        TourProgress.CheckPosition(execution, tour, Galleries, CheckedAt);

        execution.CompletedKeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Completing_the_last_key_point_ends_the_execution_as_completed_with_its_completion_time()
    {
        var tour = CreatePublishedTourWithThreeKeyPoints();
        var execution = CreateActiveExecution(tour);
        execution.CompleteKeyPoint(tour.KeyPoints[0].Id, StartedAt.AddMinutes(5), false);
        execution.CompleteKeyPoint(tour.KeyPoints[1].Id, StartedAt.AddMinutes(15), false);

        TourProgress.CheckPosition(execution, tour, UpperPlateau, CheckedAt);

        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.EndedAt.Should().Be(CheckedAt);
        execution.CompletedKeyPoints.Select(completed => completed.KeyPointId).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
    }
}
