using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static TimeWindow CreateWindow() => new(new TimeOnly(18, 0), new TimeOnly(20, 0));

    private static SocialChallenge CreateChallenge() =>
        new("Okupljanje na zalasku", "Bravo!", 5, 200, 50, CreateWindow());

    private static SocialChallenge CreateChallengeIn(ChallengeStatus status)
    {
        var challenge = CreateChallenge();
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
    public void New_challenge_starts_as_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_challenge_requires_a_name(string name)
    {
        var creation = () => new SocialChallenge(name, "Bravo!", 5, 200, 50, CreateWindow());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_challenge_requires_an_exit_message(string exitMessage)
    {
        var creation = () => new SocialChallenge("Okupljanje na zalasku", exitMessage, 5, 200, 50, CreateWindow());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void New_challenge_requires_at_least_one_person(int requiredPeople)
    {
        var creation = () => new SocialChallenge("Okupljanje na zalasku", "Bravo!", requiredPeople, 200, 50, CreateWindow());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void New_challenge_requires_a_range_greater_than_zero(int rangeInMeters)
    {
        var creation = () => new SocialChallenge("Okupljanje na zalasku", "Bravo!", 5, rangeInMeters, 50, CreateWindow());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void New_challenge_requires_xp_greater_than_zero(int xp)
    {
        var creation = () => new SocialChallenge("Okupljanje na zalasku", "Bravo!", 5, 200, xp, CreateWindow());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("   ", "Bravo!", 5, 200, 50)]
    [InlineData("Okupljanje", "   ", 5, 200, 50)]
    [InlineData("Okupljanje", "Bravo!", 0, 200, 50)]
    [InlineData("Okupljanje", "Bravo!", 5, 0, 50)]
    [InlineData("Okupljanje", "Bravo!", 5, 200, 0)]
    public void Changing_a_challenge_cannot_write_an_invalid_value_into_it(string name, string exitMessage, int requiredPeople, int rangeInMeters, int xp)
    {
        var challenge = CreateChallenge();

        var update = () => challenge.Update(name, exitMessage, requiredPeople, rangeInMeters, xp, CreateWindow());

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Okupljanje na zalasku");
        challenge.ExitMessage.Should().Be("Bravo!");
        challenge.RequiredPeople.Should().Be(5);
        challenge.RangeInMeters.Should().Be(200);
        challenge.Xp.Should().Be(50);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Challenge_that_is_not_a_draft_cannot_be_changed(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var update = () => challenge.Update("Novo ime", "Nova poruka", 2, 100, 10, CreateWindow());

        update.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Okupljanje na zalasku");
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_retired_or_archived_challenge_becomes_published(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        challenge.Publish();

        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_challenge_cannot_be_published_again()
    {
        var challenge = CreateChallengeIn(ChallengeStatus.Published);

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_challenge_becomes_retired()
    {
        var challenge = CreateChallengeIn(ChallengeStatus.Published);

        challenge.Retire();

        challenge.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_challenge_can_be_retired(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var retiring = () => challenge.Retire();

        retiring.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_challenge_becomes_archived(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        challenge.Archive();

        challenge.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_or_retired_challenge_can_be_archived(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var archiving = () => challenge.Archive();

        archiving.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_or_archived_challenge_can_be_deleted(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_challenge_cannot_be_deleted(ChallengeStatus status)
    {
        var challenge = CreateChallengeIn(status);

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
        challenge.Status.Should().Be(status);
    }
}
