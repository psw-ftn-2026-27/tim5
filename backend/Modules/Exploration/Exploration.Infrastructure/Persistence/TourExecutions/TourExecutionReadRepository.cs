using Exploration.Application.TourExecution;
using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionReadRepository : ITourExecutionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The next key point is the one with the lowest order, the same rule as Tour.FirstKeyPoint().
    public Task<ActiveTourExecutionDto?> GetActiveAsync(Guid touristId)
    {
        return _dbContext.TourExecutions
            .Where(execution => execution.TouristId == touristId && execution.Status == TourExecutionStatus.Active)
            .Join(
                _dbContext.Tours,
                execution => execution.TourId,
                tour => tour.Id,
                (execution, tour) => new { Execution = execution, Tour = tour })
            .Join(
                _dbContext.TouristPositions,
                executionTour => executionTour.Execution.TouristId,
                position => position.UserId,
                (executionTour, position) => new ActiveTourExecutionDto(
                    executionTour.Tour.Id,
                    executionTour.Tour.Name,
                    new LocationDto(position.Location.Latitude, position.Location.Longitude),
                    executionTour.Tour.KeyPoints
                        .OrderBy(keyPoint => keyPoint.Order)
                        .Select(keyPoint => new NextKeyPointDto(
                            keyPoint.Name,
                            new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                        .First()))
            .FirstOrDefaultAsync();
    }
}
