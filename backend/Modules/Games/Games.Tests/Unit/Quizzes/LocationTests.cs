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

    // One degree of latitude, and one degree of longitude on the equator, is about 111 195 m;
    // Liberty Square to Danube Park in Novi Sad is about 446 m.
    [Theory]
    [InlineData(0, 0, 1, 0, 111_195)]
    [InlineData(0, 0, 0, 1, 111_195)]
    [InlineData(45.2551, 19.8451, 45.2550, 19.8508, 446)]
    public void Distance_between_two_known_locations_matches_the_real_distance_in_meters(
        double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, double expectedMeters)
    {
        var from = new Location(fromLatitude, fromLongitude);

        var distance = from.DistanceTo(new Location(toLatitude, toLongitude));

        distance.Should().BeApproximately(expectedMeters, expectedMeters * 0.01);
    }
}
