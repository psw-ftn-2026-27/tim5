using FluentAssertions;
using Games.Domain.LocationChallengeActivations;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeActivations;

public class LocationChallengeActivationTests
{
    private static readonly Location StartLocation = new(45.2517, 19.8628);
    private static readonly DateTime Now = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Discovering_in_a_completed_challenge_is_rejected()
    {
        var challenge = new LocationChallenge("Skrivena mesta tvrđave", 70, StartLocation);
        challenge.AddHiddenPoint(Guid.NewGuid(), new Location(45.2522, 19.8615));
        challenge.Publish();
        var activation = challenge.Activate(Guid.NewGuid(), StartLocation, 10);
        var point = challenge.HiddenPoints[0];
        activation.Discover(point.Id, 1, challenge.Xp, Now.AddMinutes(-10));

        var discovering = () => activation.Discover(point.Id, 1, challenge.Xp, Now);

        discovering.Should().Throw<DomainException>();
        activation.Completion.Should().Be(new Completion(Now.AddMinutes(-10), challenge.Xp));
    }
}
