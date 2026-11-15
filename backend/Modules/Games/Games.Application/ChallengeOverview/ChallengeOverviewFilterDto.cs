using Games.Domain.SocialChallenges;

namespace Games.Application.ChallengeOverview;

public sealed record ChallengeOverviewFilterDto(ChallengeStatus? Status);
