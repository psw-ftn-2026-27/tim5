using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionTests
{
    private static readonly DateTime StartedAt = new(2026, 11, 17, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime AbandonedAt = new(2026, 11, 17, 10, 0, 0, DateTimeKind.Utc);

    private static TourExecution CreateActiveExecution() =>
        new(WellKnownUsers.SecondExplorer, Guid.NewGuid(), new Location(45.2551, 19.8451), StartedAt);

    [Fact]
    public void Abandoned_execution_is_marked_as_abandoned_with_its_abandoning_time()
    {
        var execution = CreateActiveExecution();

        execution.Abandon(AbandonedAt);

        execution.Status.Should().Be(TourExecutionStatus.Abandoned);
        execution.EndedAt.Should().Be(AbandonedAt);
    }

    [Fact]
    public void Abandoned_execution_cannot_be_abandoned_again()
    {
        var execution = CreateActiveExecution();
        execution.Abandon(AbandonedAt);

        var abandoning = () => execution.Abandon(AbandonedAt.AddHours(1));

        abandoning.Should().Throw<DomainException>();
        execution.EndedAt.Should().Be(AbandonedAt);
    }

    [Fact]
    public void Position_check_on_an_execution_that_is_no_longer_active_is_rejected()
    {
        var execution = CreateActiveExecution();
        execution.Abandon(AbandonedAt);

        var recording = () => execution.RecordActivity(AbandonedAt.AddMinutes(10));

        recording.Should().Throw<DomainException>();
        execution.LastActivity.Should().Be(StartedAt);
    }
}
