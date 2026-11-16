using Exploration.Application.TouristPositions;
using Exploration.Contracts;

namespace Exploration.Application;

public sealed class ExplorationApi : IExplorationApi
{
    private readonly ITouristPositionReadRepository _positionReadRepository;

    public ExplorationApi(ITouristPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public async Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId)
    {
        var position = await _positionReadRepository.GetByUserAsync(userId);
        if (position is null)
        {
            return null;
        }

        return new UserPositionDto(position.Latitude, position.Longitude, position.MarkedAt);
    }
}
