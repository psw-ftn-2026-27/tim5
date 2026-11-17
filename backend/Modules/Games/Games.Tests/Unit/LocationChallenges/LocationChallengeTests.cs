using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeTests
{
    private static Location CreateStartLocation() => new(45.2517, 19.8628);

    private static LocationChallenge CreateChallenge() => new("Skrivena mesta tvrđave", 70, CreateStartLocation());

    private static LocationChallenge CreateChallengeWithOnePoint(Guid imageId, Location location)
    {
        var challenge = CreateChallenge();
        challenge.AddHiddenPoint(imageId, location);
        return challenge;
    }

    [Fact]
    public void New_challenge_starts_as_a_draft_without_hidden_points()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(ChallengeStatus.Draft);
        challenge.HiddenPoints.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_challenge_requires_a_name(string name)
    {
        var creation = () => new LocationChallenge(name, 70, CreateStartLocation());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void New_challenge_requires_xp_greater_than_zero(int xp)
    {
        var creation = () => new LocationChallenge("Skrivena mesta tvrđave", xp, CreateStartLocation());

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_challenge_requires_a_start_location()
    {
        var creation = () => new LocationChallenge("Skrivena mesta tvrđave", 70, null!);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("   ", 70)]
    [InlineData("Novo ime", 0)]
    public void Changing_a_challenge_cannot_write_an_invalid_name_or_xp_into_it(string name, int xp)
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update(name, xp, new Location(45.0, 19.0));

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Skrivena mesta tvrđave");
        challenge.Xp.Should().Be(70);
        challenge.StartLocation.Should().Be(CreateStartLocation());
    }

    [Fact]
    public void Changing_a_challenge_cannot_remove_its_start_location()
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update("Novo ime", 30, null!);

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Skrivena mesta tvrđave");
        challenge.StartLocation.Should().Be(CreateStartLocation());
    }

    [Fact]
    public void Hidden_point_is_not_added_without_a_photo()
    {
        var challenge = CreateChallenge();

        var adding = () => challenge.AddHiddenPoint(Guid.Empty, new Location(45.2522, 19.8615));

        adding.Should().Throw<DomainException>();
        challenge.HiddenPoints.Should().BeEmpty();
    }

    [Fact]
    public void Hidden_point_is_not_added_without_a_location()
    {
        var challenge = CreateChallenge();

        var adding = () => challenge.AddHiddenPoint(Guid.NewGuid(), null!);

        adding.Should().Throw<DomainException>();
        challenge.HiddenPoints.Should().BeEmpty();
    }

    [Fact]
    public void Hidden_point_is_not_changed_to_have_no_photo()
    {
        var imageId = Guid.NewGuid();
        var location = new Location(45.2522, 19.8615);
        var challenge = CreateChallengeWithOnePoint(imageId, location);
        var point = challenge.HiddenPoints[0];

        var change = () => challenge.ChangeHiddenPoint(point.Id, Guid.Empty, new Location(45.2509, 19.8641));

        change.Should().Throw<DomainException>();
        point.ImageId.Should().Be(imageId);
        point.Location.Should().Be(location);
    }

    [Fact]
    public void Hidden_point_is_not_changed_to_have_no_location()
    {
        var imageId = Guid.NewGuid();
        var location = new Location(45.2522, 19.8615);
        var challenge = CreateChallengeWithOnePoint(imageId, location);
        var point = challenge.HiddenPoints[0];

        var change = () => challenge.ChangeHiddenPoint(point.Id, Guid.NewGuid(), null!);

        change.Should().Throw<DomainException>();
        point.ImageId.Should().Be(imageId);
        point.Location.Should().Be(location);
    }

    [Fact]
    public void Changing_a_hidden_point_that_is_not_in_the_challenge_is_rejected()
    {
        var imageId = Guid.NewGuid();
        var location = new Location(45.2522, 19.8615);
        var challenge = CreateChallengeWithOnePoint(imageId, location);

        var change = () => challenge.ChangeHiddenPoint(Guid.NewGuid(), Guid.NewGuid(), new Location(45.2509, 19.8641));

        change.Should().Throw<NotFoundException>();
        challenge.HiddenPoints.Should().ContainSingle()
            .Which.ImageId.Should().Be(imageId);
    }

    [Fact]
    public void Removing_a_hidden_point_that_is_not_in_the_challenge_is_rejected()
    {
        var challenge = CreateChallengeWithOnePoint(Guid.NewGuid(), new Location(45.2522, 19.8615));

        var removal = () => challenge.RemoveHiddenPoint(Guid.NewGuid());

        removal.Should().Throw<NotFoundException>();
        challenge.HiddenPoints.Should().HaveCount(1);
    }
}
