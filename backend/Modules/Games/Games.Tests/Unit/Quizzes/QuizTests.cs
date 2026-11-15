using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private static Location CreateLocation() => new(45.2517, 19.8628);

    private static List<Question> CreateQuestions() =>
        [new Question("Koliko hodnika ima tvrđava?", [new Answer("Oko 16 km", true), new Answer("Oko 2 km", false)])];

    private static Quiz CreateQuiz() =>
        new("Tvrđava", "Odgovorite na pitanja.", 50, CreateLocation(), CreateQuestions());

    private static Quiz CreateQuizIn(ChallengeStatus status)
    {
        var quiz = CreateQuiz();
        switch (status)
        {
            case ChallengeStatus.Published:
                quiz.Publish();
                break;
            case ChallengeStatus.Retired:
                quiz.Publish();
                quiz.Retire();
                break;
            case ChallengeStatus.Archived:
                quiz.Publish();
                quiz.Archive();
                break;
        }
        return quiz;
    }

    [Fact]
    public void New_quiz_starts_as_a_draft()
    {
        var quiz = new Quiz("Tvrđava", "Odgovorite na pitanja.", 50, CreateLocation(), CreateQuestions());

        quiz.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public void Quiz_without_questions_cannot_be_created()
    {
        var creation = () => new Quiz("Tvrđava", "Odgovorite na pitanja.", 50, CreateLocation(), []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Odgovorite na pitanja.", 50)]
    [InlineData("   ", "Odgovorite na pitanja.", 50)]
    [InlineData("Tvrđava", "", 50)]
    [InlineData("Tvrđava", "   ", 50)]
    [InlineData("Tvrđava", "Odgovorite na pitanja.", 0)]
    [InlineData("Tvrđava", "Odgovorite na pitanja.", -10)]
    public void Quiz_without_a_name_or_instructions_or_with_xp_not_greater_than_zero_cannot_be_created(string name, string instructions, int xp)
    {
        var creation = () => new Quiz(name, instructions, xp, CreateLocation(), CreateQuestions());

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Quiz_that_is_not_a_draft_cannot_be_changed(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        var update = () => quiz.Update("Novo ime", "Nova uputstva", 10, CreateLocation(), CreateQuestions());

        update.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Tvrđava");
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_retired_or_archived_quiz_becomes_published(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        quiz.Publish();

        quiz.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_quiz_cannot_be_published_again()
    {
        var quiz = CreateQuizIn(ChallengeStatus.Published);

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
        quiz.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_quiz_becomes_retired()
    {
        var quiz = CreateQuizIn(ChallengeStatus.Published);

        quiz.Retire();

        quiz.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Retired)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_quiz_can_be_retired(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        var retiring = () => quiz.Retire();

        retiring.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_quiz_becomes_archived(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        quiz.Archive();

        quiz.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Only_a_published_or_retired_quiz_can_be_archived(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        var archiving = () => quiz.Archive();

        archiving.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(ChallengeStatus.Draft)]
    [InlineData(ChallengeStatus.Archived)]
    public void Draft_or_archived_quiz_can_be_deleted(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.Delete();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(ChallengeStatus.Published)]
    [InlineData(ChallengeStatus.Retired)]
    public void Published_or_retired_quiz_cannot_be_deleted(ChallengeStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.Delete();

        deletion.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }
}
