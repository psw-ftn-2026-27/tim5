using Games.Domain.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string Instructions { get; private set; }
    public int Xp { get; private set; }
    public Location Location { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;
    public ChallengeStatus Status { get; private set; }

    private Quiz()
    {
        Name = null!;
        Instructions = null!;
        Location = null!;
    }

    public Quiz(string name, string instructions, int xp, Location location, List<Question> questions)
        : base(Guid.NewGuid())
    {
        Validate(name, instructions, xp, location, questions);

        Name = name;
        Instructions = instructions;
        Xp = xp;
        Location = location;
        _questions.AddRange(questions);
        Status = ChallengeStatus.Draft;
    }

    public void Update(string name, string instructions, int xp, Location location, List<Question> questions)
    {
        if (Status != ChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be changed.");
        }
        Validate(name, instructions, xp, location, questions);

        Name = name;
        Instructions = instructions;
        Xp = xp;
        Location = location;
        _questions.Clear();
        _questions.AddRange(questions);
    }

    public void Publish()
    {
        if (Status == ChallengeStatus.Published)
        {
            throw new DomainException("The quiz is already published.");
        }

        Status = ChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != ChallengeStatus.Published)
        {
            throw new DomainException("Only a published quiz can be retired.");
        }

        Status = ChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != ChallengeStatus.Published && Status != ChallengeStatus.Retired)
        {
            throw new DomainException("Only a published or retired quiz can be archived.");
        }

        Status = ChallengeStatus.Archived;
    }

    public void Delete()
    {
        if (Status != ChallengeStatus.Draft && Status != ChallengeStatus.Archived)
        {
            throw new DomainException("Only a draft or archived quiz can be deleted.");
        }
    }

    private static void Validate(string name, string instructions, int xp, Location location, List<Question> questions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A quiz requires a name.");
        }
        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new DomainException("A quiz requires instructions.");
        }
        if (xp <= 0)
        {
            throw new DomainException("A quiz requires an XP reward greater than zero.");
        }
        if (location is null)
        {
            throw new DomainException("A quiz requires a location.");
        }
        if (questions is null || questions.Count == 0)
        {
            throw new DomainException("A quiz requires at least one question.");
        }
    }
}
