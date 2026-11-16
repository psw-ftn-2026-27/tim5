using Exploration.Domain.TouristPositions;

namespace Exploration.Application.TouristPositions;

public interface ITouristPositionRepository
{
    Task<TouristPosition?> GetByUserAsync(Guid userId);
    void Add(TouristPosition position);
}
