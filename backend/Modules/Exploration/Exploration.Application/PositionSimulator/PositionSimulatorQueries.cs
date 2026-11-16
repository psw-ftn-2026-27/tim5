using Exploration.Application.TouristPositions;
using Shared.Domain.Exceptions;

namespace Exploration.Application.PositionSimulator;

public sealed class PositionSimulatorQueries
{
    private readonly ITouristPositionReadRepository _positionReadRepository;

    public PositionSimulatorQueries(ITouristPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public async Task<TouristPositionDto> GetMyPositionAsync(Guid userId)
    {
        var position = await _positionReadRepository.GetByUserAsync(userId);
        if (position is null)
        {
            throw new NotFoundException("The position has not been marked yet.");
        }

        return position;
    }
}
