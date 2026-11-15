using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringCommandTests : BaseIntegrationTest
{
    public QuizAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static QuizInputDto CreateValidInput() =>
        new("Most slobode", "Pronađite odgovore ispod mosta.", 40, new LocationDto(45.2386, 19.8650),
        [
            new QuestionDto("Koje godine je most otvoren?", [new AnswerDto("1981", true), new AnswerDto("1999", false)]),
            new QuestionDto("Kakav je most po konstrukciji?", [new AnswerDto("Lučni", false), new AnswerDto("Kosi viseći", true), new AnswerDto("Pontonski", false)])
        ]);

    [Fact]
    public async Task Administrator_creates_a_quiz_that_is_stored_as_a_draft_with_all_questions_and_answers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<QuizDto>(JsonOptions);
        created!.Status.Should().Be(ChallengeStatus.Draft);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == created.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.Xp.Should().Be(request.Xp);
        stored.Location.Should().Be(new Location(request.Location.Latitude, request.Location.Longitude));
        stored.Questions.Should().BeEquivalentTo(request.Questions, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Quiz_with_invalid_data_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput() with { Questions = [] };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/quizzes", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_changes_a_draft_quiz_and_the_new_values_including_questions_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftFortressQuiz.Id);
        stored.Name.Should().Be(request.Name);
        stored.Instructions.Should().Be(request.Instructions);
        stored.Xp.Should().Be(request.Xp);
        stored.Location.Should().Be(new Location(request.Location.Latitude, request.Location.Longitude));
        stored.Questions.Should().BeEquivalentTo(request.Questions, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Changing_a_quiz_that_does_not_exist_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingId = Guid.NewGuid();
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{missingId}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == missingId);
    }

    [Fact]
    public async Task Changing_a_quiz_with_invalid_data_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateValidInput() with { Questions = [] };

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftFortressQuiz.Id);
        stored.Name.Should().Be(QuizSeed.DraftFortressQuiz.Name);
        stored.Questions.Should().HaveCount(QuizSeed.DraftFortressQuiz.Questions.Count);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_change_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", CreateValidInput());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
