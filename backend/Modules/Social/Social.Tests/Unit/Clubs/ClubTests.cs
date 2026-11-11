using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Clubs;
using Xunit;

namespace Social.Tests.Unit.Clubs;

public class ClubTests
{
    private static readonly Guid MountainImageId = Guid.NewGuid();

    private static Club CreateClub() =>
        new(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), "Vikend usponi na Frušku goru.", [MountainImageId]);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Club_cannot_be_founded_without_a_name(string name)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, new ClubName(name), "Vikend usponi na Frušku goru.", [Guid.NewGuid()]);

        founding.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Club_cannot_be_founded_without_a_description(string description)
    {
        var founding = () => new Club(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), description, [Guid.NewGuid()]);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void Club_cannot_be_founded_without_an_image()
    {
        var founding = () => new Club(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), "Vikend usponi na Frušku goru.", []);

        founding.Should().Throw<DomainException>();
    }

    [Fact]
    public void Update_removing_all_images_is_rejected_and_the_club_keeps_its_images()
    {
        var club = CreateClub();

        var update = () => club.UpdateDetails(WellKnownUsers.Explorer, new ClubName("Planinari Fruške gore"), "Novi opis.", []);

        update.Should().Throw<DomainException>();
        club.ImageIds.Should().Equal(MountainImageId);
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_update_the_club()
    {
        var club = CreateClub();

        var update = () => club.UpdateDetails(WellKnownUsers.SecondExplorer, new ClubName("Tuđi klub"), "Tuđi opis.", [Guid.NewGuid()]);

        update.Should().Throw<DomainException>();
        club.Name.Value.Should().Be("Planinari Fruške gore");
    }

    [Fact]
    public void User_who_is_not_the_owner_cannot_delete_the_club()
    {
        var club = CreateClub();

        var deletion = () => club.Delete(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<DomainException>();
    }
}
