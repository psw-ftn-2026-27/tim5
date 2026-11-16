using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class LocationTests
{
    [Theory]
    [InlineData(-90.1, 19.8)]
    [InlineData(90.1, 19.8)]
    [InlineData(45.2, -180.1)]
    [InlineData(45.2, 180.1)]
    public void Location_with_a_latitude_or_longitude_out_of_range_cannot_exist(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
