using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubNameUniquenessTests
{
    private static Club CreateClub(Guid ownerId, string name) =>
        new(ownerId, new ClubName(name), "Opis kluba.", [new ClubImage("https://example.com/klub.jpg")]);

    [Theory]
    [InlineData("Planinari Fruške gore")]
    [InlineData("planinari fruške gore")]
    [InlineData("  PLANINARI FRUŠKE GORE  ")]
    public void Name_held_by_another_club_is_rejected_for_a_new_club(string name)
    {
        var existingClub = CreateClub(WellKnownUsers.Explorer, "Planinari Fruške gore");

        var check = () => new ClubNameUniqueness().CheckForNewClub(new ClubName(name), existingClub);

        check.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("Planinari Fruške gore")]
    [InlineData("  planinari FRUŠKE gore ")]
    public void Name_held_by_another_club_is_rejected_on_rename(string name)
    {
        var existingClub = CreateClub(WellKnownUsers.Explorer, "Planinari Fruške gore");
        var renamedClub = CreateClub(WellKnownUsers.SecondExplorer, "Fotografi Novog Sada");

        var check = () => new ClubNameUniqueness().CheckForRename(renamedClub, new ClubName(name), existingClub);

        check.Should().Throw<DomainException>();
    }

    [Fact]
    public void Owner_may_keep_the_name_of_their_club_on_update()
    {
        var club = CreateClub(WellKnownUsers.Explorer, "Planinari Fruške gore");

        var check = () => new ClubNameUniqueness().CheckForRename(club, new ClubName("Planinari Fruške gore"), club);

        check.Should().NotThrow();
    }
}
