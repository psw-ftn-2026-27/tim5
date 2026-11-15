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
}
