using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record Question
{
    private readonly List<Answer> _answers = [];

    public string Text { get; private init; }
    public IReadOnlyList<Answer> Answers => _answers;

    private Question()
    {
        Text = null!;
    }

    public Question(string text, List<Answer> answers)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A question requires a text.");
        }
        if (answers is null || answers.Count < 2)
        {
            throw new DomainException("A question requires at least two answers.");
        }
        if (answers.Any(answer => answer is null || string.IsNullOrWhiteSpace(answer.Text)))
        {
            throw new DomainException("Every answer requires a text.");
        }
        if (answers.Count(answer => answer.IsCorrect) != 1)
        {
            throw new DomainException("A question requires exactly one correct answer.");
        }

        Text = text;
        _answers.AddRange(answers);
    }
}
