using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public class LocationChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public LocationChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static LocationChallengeInputDto CreateValidInput() =>
        new("Skrivena mesta Dunavskog parka", 35, new LocationDto(45.2550, 19.8508));

    [Fact]
    public async Task Administrator_creates_a_challenge_that_is_stored_as_a_draft_without_hidden_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<LocationChallengeDto>(JsonOptions);
        created!.Status.Should().Be(ChallengeStatus.Draft);
        created.HiddenPoints.Should().BeEmpty();
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == created.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.Xp.Should().Be(request.Xp);
        stored.StartLocation.Should().Be(new Location(request.StartLocation.Latitude, request.StartLocation.Longitude));
        stored.HiddenPoints.Should().BeEmpty();
    }

    [Fact]
    public async Task Challenge_with_invalid_data_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput() with { Xp = 0 };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/location-challenges", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_the_basic_data_of_a_draft_challenge_and_the_new_values_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challengeId = LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id;
        var request = CreateValidInput();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{challengeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(challenge => challenge.Id == challengeId);
        stored.Name.Should().Be(request.Name);
        stored.Xp.Should().Be(request.Xp);
        stored.StartLocation.Should().Be(new Location(request.StartLocation.Latitude, request.StartLocation.Longitude));
    }

    [Fact]
    public async Task Changing_a_challenge_that_does_not_exist_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.LocationChallenges.Count();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{missingId}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.LocationChallenges.Count().Should().Be(challengeCountBefore);
        assertContext.LocationChallenges.Should().NotContain(challenge => challenge.Id == missingId);
    }

    [Fact]
    public async Task Changing_a_challenge_with_invalid_data_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = LocationChallengeSeed.DraftFortressHuntWithThreePoints;
        var request = CreateValidInput() with { Name = "" };

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{challenge.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.LocationChallenges.Single(stored => stored.Id == challenge.Id);
        stored.Name.Should().Be(challenge.Name);
        stored.Xp.Should().Be(challenge.Xp);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/location-challenges/{LocationChallengeSeed.DraftFortressHuntWithThreePoints.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
