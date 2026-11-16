using Exploration.Application.PositionSimulator;
using Exploration.Application.TouristPositions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TouristPositions;

internal sealed class TouristPositionReadRepository : ITouristPositionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TouristPositionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TouristPositionDto?> GetByUserAsync(Guid userId)
    {
        return _dbContext.TouristPositions
            .Where(position => position.UserId == userId)
            .Select(position => new TouristPositionDto(
                position.Location.Latitude,
                position.Location.Longitude,
                position.MarkedAt))
            .FirstOrDefaultAsync();
    }
}
