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

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    [InlineData(-90, 180)]
    [InlineData(90, -180)]
    public void Location_on_the_latitude_and_longitude_boundaries_exists(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    // Liberty Square to Danube Park in Novi Sad is about 446 m; one degree of latitude is about 111 km.
    [Theory]
    [InlineData(45.2551, 19.8451, 45.2550, 19.8508, 446)]
    [InlineData(0, 0, 1, 0, 111_195)]
    public void Distance_between_two_known_locations_matches_the_real_distance_in_meters(
        double fromLatitude, double fromLongitude, double toLatitude, double toLongitude, double expectedMeters)
    {
        var from = new Location(fromLatitude, fromLongitude);

        var distance = from.DistanceTo(new Location(toLatitude, toLongitude));

        distance.Should().BeApproximately(expectedMeters, expectedMeters * 0.01);
    }
}
