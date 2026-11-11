using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<SocialChallengeDto>> GetAllAsync(ChallengeStatus? status)
    {
        var challenges = status is null
            ? _dbContext.SocialChallenges
            : _dbContext.SocialChallenges.Where(challenge => challenge.Status == status);

        return ProjectToDtos(challenges).ToListAsync();
    }

    private static IQueryable<SocialChallengeDto> ProjectToDtos(IQueryable<SocialChallenge> challenges)
    {
        return challenges
            .Select(challenge => new SocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.ExitMessage,
                challenge.RequiredPeople,
                challenge.RangeInMeters,
                challenge.Xp,
                new TimeWindowDto(challenge.Window.Start, challenge.Window.End),
                challenge.Status));
    }
}
