using FluentAssertions;
using Games.Domain.LocationChallengeActivations;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeActivations;

public class LocationChallengeActivatorTests
{
    private static readonly Location StartLocation = new(45.2517, 19.8628);

    private static LocationChallenge CreatePublishedChallenge()
    {
        var challenge = new LocationChallenge("Skrivena mesta tvrđave", 70, StartLocation);
        challenge.AddHiddenPoint(Guid.NewGuid(), new Location(45.2522, 19.8615));
        challenge.Publish();
        return challenge;
    }

    [Fact]
    public void User_who_already_activated_a_challenge_cannot_activate_it_again()
    {
        var challenge = CreatePublishedChallenge();
        var userId = Guid.NewGuid();
        var existingActivation = challenge.Activate(userId, StartLocation, 10);

        var activating = () => LocationChallengeActivator.Activate(challenge, existingActivation, userId, StartLocation, 20);

        activating.Should().Throw<DomainException>();
    }
}
