namespace Games.Application.SocialChallengeAuthoring;

public sealed record SocialChallengeInputDto(
    string Name,
    string ExitMessage,
    int RequiredPeople,
    int RangeInMeters,
    int Xp,
    TimeWindowDto Window);
