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

    private static LocationChallenge CreateChallengeWithOnePointIn(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePoint(Guid.NewGuid(), new Location(45.2522, 19.8615));
        switch (status)
        {
            case ChallengeStatus.Published:
                challenge.Publish();
                break;
            case ChallengeStatus.Retired:
                challenge.Publish();
                challenge.Retire();
                break;
            case ChallengeStatus.Archived:
                challenge.Publish();
                challenge.Archive();
                break;
        }
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

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_retired_or_archived_challenge_becomes_published(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        challenge.Publish();

        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_challenge_cannot_be_published_again()
    {
        var challenge = CreateChallengeWithOnePointIn(ChallengeStatus.Published);

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Draft_without_hidden_points_cannot_be_published()
    {
        var challenge = CreateChallenge();

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
        challenge.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public void Published_challenge_becomes_retired()
    {
        var challenge = CreateChallengeWithOnePointIn(ChallengeStatus.Published);

        challenge.Retire();

        challenge.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_challenge_can_be_retired(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var retiring = () => challenge.Retire();

        retiring.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_challenge_becomes_archived(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        challenge.Archive();

        challenge.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_or_retired_challenge_can_be_archived(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var archiving = () => challenge.Archive();

        archiving.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_or_archived_challenge_can_be_deleted(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_challenge_cannot_be_deleted(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Challenge_that_is_not_a_draft_cannot_be_changed(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var update = () => challenge.Update("Novo ime", 30, new Location(45.0, 19.0));

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Skrivena mesta tvrđave");
        challenge.Xp.Should().Be(70);
        challenge.StartLocation.Should().Be(CreateStartLocation());
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Hidden_point_cannot_be_added_to_a_challenge_that_is_not_a_draft(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var adding = () => challenge.AddHiddenPoint(Guid.NewGuid(), new Location(45.2509, 19.8641));

        adding.Should().Throw<DomainException>();
        challenge.HiddenPoints.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Hidden_point_of_a_challenge_that_is_not_a_draft_cannot_be_changed(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);
        var point = challenge.HiddenPoints[0];
        var imageId = point.ImageId;

        var change = () => challenge.ChangeHiddenPoint(point.Id, Guid.NewGuid(), new Location(45.2509, 19.8641));

        change.Should().Throw<DomainException>();
        point.ImageId.Should().Be(imageId);
        point.Location.Should().Be(new Location(45.2522, 19.8615));
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Hidden_point_of_a_challenge_that_is_not_a_draft_cannot_be_removed(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var removal = () => challenge.RemoveHiddenPoint(challenge.HiddenPoints[0].Id);

        removal.Should().Throw<DomainException>();
        challenge.HiddenPoints.Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    public void Published_challenge_is_activated_from_up_to_50_meters_of_its_start_location(double meters)
    {
        var challenge = CreateChallengeWithOnePointIn(ChallengeStatus.Published);
        var userId = Guid.NewGuid();

        var activation = challenge.Activate(userId, NorthOfStart(meters), 0);

        activation.UserId.Should().Be(userId);
        activation.ChallengeId.Should().Be(challenge.Id);
    }

    [Fact]
    public void Challenge_is_not_activated_from_51_meters_of_its_start_location()
    {
        var challenge = CreateChallengeWithOnePointIn(ChallengeStatus.Published);

        var activating = () => challenge.Activate(Guid.NewGuid(), NorthOfStart(51), 0);

        activating.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Challenge_that_is_not_published_is_not_activated(ChallengeStatus status)
    {
        var challenge = CreateChallengeWithOnePointIn(status);

        var activating = () => challenge.Activate(Guid.NewGuid(), CreateStartLocation(), 0);

        activating.Should().Throw<DomainException>();
    }

    [Fact]
    public void Radius_of_the_circle_is_the_distance_to_the_farthest_hidden_point_plus_the_extra_meters()
    {
        var challenge = CreateChallenge();
        challenge.AddHiddenPoint(Guid.NewGuid(), NorthOfStart(30));
        challenge.AddHiddenPoint(Guid.NewGuid(), NorthOfStart(80));
        challenge.Publish();

        var activation = challenge.Activate(Guid.NewGuid(), CreateStartLocation(), 25);

        activation.Radius.Should().BeApproximately(105, 0.01);
    }

    // One degree of latitude is about 111 195 m, a little more than the haversine formula gives,
    // so the location lands a fraction of a millimetre short of the requested distance.
    private static Location NorthOfStart(double meters)
    {
        var start = CreateStartLocation();
        return new Location(start.Latitude + meters / 111_195, start.Longitude);
    }
}
