using Exploration.Application.TouristPositions;
using Exploration.Domain.TouristPositions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TouristPositions;

internal sealed class TouristPositionRepository : ITouristPositionRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TouristPositionRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TouristPosition?> GetByUserAsync(Guid userId)
    {
        return _dbContext.TouristPositions.FirstOrDefaultAsync(position => position.UserId == userId);
    }

    public void Add(TouristPosition position)
    {
        _dbContext.TouristPositions.Add(position);
    }
}
