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

    // The next key point is the uncompleted one with the lowest order, the same rule as Tour.NextKeyPoint().
    // Only completed key points carry their secret.
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
                        .Where(keyPoint => !executionTour.Execution.CompletedKeyPoints.Any(completed => completed.KeyPointId == keyPoint.Id))
                        .OrderBy(keyPoint => keyPoint.Order)
                        .Select(keyPoint => new NextKeyPointDto(
                            keyPoint.Name,
                            new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                        .First(),
                    executionTour.Execution.CompletedKeyPoints
                        .OrderBy(completed => completed.ReachedAt)
                        .Select(completed => new CompletedKeyPointDto(
                            completed.KeyPointId,
                            executionTour.Tour.KeyPoints.First(keyPoint => keyPoint.Id == completed.KeyPointId).Name,
                            completed.ReachedAt,
                            executionTour.Tour.KeyPoints.First(keyPoint => keyPoint.Id == completed.KeyPointId).Secret))
                        .ToList()))
            .FirstOrDefaultAsync();
    }
}
