using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuestionTests
{
    [Fact]
    public void Question_with_fewer_than_two_answers_cannot_be_created()
    {
        var creation = () => new Question("Koliko hodnika ima tvrđava?", [new Answer("Oko 16 km", true)]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Question_without_a_text_cannot_be_created(string text)
    {
        var creation = () => new Question(text, [new Answer("Oko 16 km", true), new Answer("Oko 2 km", false)]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Question_with_an_answer_without_a_text_cannot_be_created(string answerText)
    {
        var creation = () => new Question("Koliko hodnika ima tvrđava?", [new Answer("Oko 16 km", true), new Answer(answerText, false)]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_without_a_correct_answer_cannot_be_created()
    {
        var creation = () => new Question("Koliko hodnika ima tvrđava?", [new Answer("Oko 16 km", false), new Answer("Oko 2 km", false)]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Question_with_more_than_one_correct_answer_cannot_be_created()
    {
        var creation = () => new Question("Koliko hodnika ima tvrđava?", [new Answer("Oko 16 km", true), new Answer("Oko 2 km", true), new Answer("Oko 50 km", false)]);

        creation.Should().Throw<DomainException>();
    }
}
