using Exploration.Application.PositionSimulator;

namespace Exploration.Application.TouristPositions;

public interface ITouristPositionReadRepository
{
    Task<TouristPositionDto?> GetByUserAsync(Guid userId);
}
