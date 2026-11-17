using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionStarterTests
{
    private static readonly Location StartPosition = new(45.2551, 19.8451);
    private static readonly DateTime StartedAt = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);

    private static Tour CreatePublishedTour()
    {
        var tour = new Tour(WellKnownUsers.Explorer, "Stari grad", new string('o', 100), TourDifficulty.Easy, ["grad"], new GroupSize(1, 10));
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.AddKeyPoint("Trg", "Opis tačke.", Guid.NewGuid(), null, true, new Location(45.25, 19.84));
        tour.AddKeyPoint("Park", "Opis tačke.", Guid.NewGuid(), null, false, new Location(45.26, 19.85));
        tour.Publish(new Price(true, 0));
        return tour;
    }

    [Fact]
    public void Tour_that_is_not_purchased_cannot_be_started()
    {
        var tour = CreatePublishedTour();

        var start = () => TourExecutionStarter.Start(WellKnownUsers.SecondExplorer, tour, false, false, StartPosition, StartedAt);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tourist_with_an_active_execution_cannot_start_another_tour()
    {
        var tour = CreatePublishedTour();

        var start = () => TourExecutionStarter.Start(WellKnownUsers.SecondExplorer, tour, true, true, StartPosition, StartedAt);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Started_execution_is_active_with_its_start_position_and_start_time()
    {
        var tour = CreatePublishedTour();

        var execution = TourExecutionStarter.Start(WellKnownUsers.SecondExplorer, tour, true, false, StartPosition, StartedAt);

        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.TouristId.Should().Be(WellKnownUsers.SecondExplorer);
        execution.TourId.Should().Be(tour.Id);
        execution.StartPosition.Should().Be(StartPosition);
        execution.StartedAt.Should().Be(StartedAt);
        execution.EndedAt.Should().BeNull();
    }
}
