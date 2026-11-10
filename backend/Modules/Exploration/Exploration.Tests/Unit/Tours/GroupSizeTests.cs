using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class GroupSizeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Group_size_minimum_must_be_at_least_one(int min)
    {
        var creation = () => new GroupSize(min, 5);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Group_size_maximum_cannot_be_less_than_the_minimum()
    {
        var creation = () => new GroupSize(4, 3);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Group_size_with_equal_bounds_is_allowed()
    {
        var groupSize = new GroupSize(3, 3);

        groupSize.Min.Should().Be(3);
        groupSize.Max.Should().Be(3);
    }
}
