using FluentAssertions;
using Games.Domain.LocationChallengeActivations;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeActivations;

public class LocationCheckerTests
{
    // One degree of latitude on the sphere Location.DistanceTo uses.
    private const double MetersPerDegreeOfLatitude = 6_371_000 * Math.PI / 180;

    private static readonly Location StartLocation = new(45.2517, 19.8628);
    private static readonly DateTime Now = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    private static LocationChallenge CreatePublishedChallengeWithTwoPoints()
    {
        var challenge = new LocationChallenge("Skrivena mesta tvrđave", 70, StartLocation);
        challenge.AddHiddenPoint(Guid.NewGuid(), new Location(45.2522, 19.8615));
        challenge.AddHiddenPoint(Guid.NewGuid(), new Location(45.2509, 19.8641));
        challenge.Publish();
        return challenge;
    }

    private static LocationChallengeActivation Activate(LocationChallenge challenge)
    {
        return challenge.Activate(Guid.NewGuid(), StartLocation, 10);
    }

    private static Location NorthOf(Location location, double meters)
    {
        return new Location(location.Latitude + meters / MetersPerDegreeOfLatitude, location.Longitude);
    }

    // 4.99 m instead of exactly 5 m: the distance is computed in floating point, so a position placed
    // exactly 5 m away may come out a fraction of a nanometer over the limit.
    [Fact]
    public void Check_just_within_5_meters_of_an_undiscovered_point_discovers_it()
    {
        var challenge = CreatePublishedChallengeWithTwoPoints();
        var activation = Activate(challenge);
        var point = challenge.HiddenPoints[0];

        LocationChecker.Check(challenge, activation, NorthOf(point.Location, 4.99), Now);

        activation.DiscoveredPointIds.Should().Equal(point.Id);
    }

    [Fact]
    public void Check_6_meters_from_an_undiscovered_point_is_a_miss()
    {
        var challenge = CreatePublishedChallengeWithTwoPoints();
        var activation = Activate(challenge);

        var checking = () => LocationChecker.Check(challenge, activation, NorthOf(challenge.HiddenPoints[0].Location, 6), Now);

        checking.Should().Throw<DomainException>();
        activation.DiscoveredPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Check_at_the_second_point_discovers_it_although_the_first_point_is_undiscovered()
    {
        var challenge = CreatePublishedChallengeWithTwoPoints();
        var activation = Activate(challenge);
        var secondPoint = challenge.HiddenPoints[1];

        LocationChecker.Check(challenge, activation, secondPoint.Location, Now);

        activation.DiscoveredPointIds.Should().Equal(secondPoint.Id);
    }

    [Fact]
    public void Check_at_an_already_discovered_point_is_a_miss()
    {
        var challenge = CreatePublishedChallengeWithTwoPoints();
        var activation = Activate(challenge);
        var firstPoint = challenge.HiddenPoints[0];
        LocationChecker.Check(challenge, activation, firstPoint.Location, Now);

        var checking = () => LocationChecker.Check(challenge, activation, firstPoint.Location, Now);

        checking.Should().Throw<DomainException>();
        activation.DiscoveredPointIds.Should().Equal(firstPoint.Id);
        activation.Completion.Should().BeNull();
    }

    [Fact]
    public void Discovering_the_last_point_records_the_completion_time_and_the_challenge_xp()
    {
        var challenge = CreatePublishedChallengeWithTwoPoints();
        var activation = Activate(challenge);
        LocationChecker.Check(challenge, activation, challenge.HiddenPoints[0].Location, Now.AddMinutes(-10));

        LocationChecker.Check(challenge, activation, challenge.HiddenPoints[1].Location, Now);

        activation.Completion.Should().Be(new Completion(Now, challenge.Xp));
    }
}
