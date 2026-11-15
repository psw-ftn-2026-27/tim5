using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class LocationTests
{
    [Theory]
    [InlineData(-90.0001, 0)]
    [InlineData(90.0001, 0)]
    [InlineData(0, -180.0001)]
    [InlineData(0, 180.0001)]
    public void Coordinates_outside_the_allowed_range_are_rejected(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Coordinates_on_the_edge_of_the_range_are_accepted(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }
}
