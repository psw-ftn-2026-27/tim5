namespace Games.Domain.Quizzes;

public sealed record Answer
{
    public string Text { get; private init; }
    public bool IsCorrect { get; private init; }

    private Answer()
    {
        Text = null!;
    }

    public Answer(string text, bool isCorrect)
    {
        Text = text;
        IsCorrect = isCorrect;
    }
}
