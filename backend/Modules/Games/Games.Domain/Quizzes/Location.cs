using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record Location
{
    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A location requires a latitude between -90 and 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A location requires a longitude between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    // The haversine formula, which treats the Earth as a sphere; precise enough for walking distances.
    public double DistanceTo(Location other)
    {
        const double earthRadiusInMeters = 6_371_000;

        var latitudeDelta = ToRadians(other.Latitude - Latitude);
        var longitudeDelta = ToRadians(other.Longitude - Longitude);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);

        return earthRadiusInMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
