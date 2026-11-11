using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class TimeWindowTests
{
    [Fact]
    public void Time_window_with_the_same_start_and_end_is_rejected()
    {
        var creation = () => new TimeWindow(new TimeOnly(18, 30), new TimeOnly(18, 30));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Time_window_that_crosses_midnight_is_accepted()
    {
        var window = new TimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0));

        window.Start.Should().Be(new TimeOnly(22, 0));
        window.End.Should().Be(new TimeOnly(2, 0));
    }
}
