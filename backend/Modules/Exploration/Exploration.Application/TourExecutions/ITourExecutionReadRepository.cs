using Exploration.Application.TourExecution;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    Task<ActiveTourExecutionDto?> GetActiveAsync(Guid touristId);
}
