using Exploration.Application.TouristPositions;
using Exploration.Application.Tours;
using Exploration.Contracts;

namespace Exploration.Application;

public sealed class ExplorationApi : IExplorationApi
{
    private readonly ITouristPositionReadRepository _positionReadRepository;
    private readonly ITourReadRepository _tourReadRepository;

    public ExplorationApi(ITouristPositionReadRepository positionReadRepository, ITourReadRepository tourReadRepository)
    {
        _positionReadRepository = positionReadRepository;
        _tourReadRepository = tourReadRepository;
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

    public async Task<IReadOnlyCollection<Guid>> GetPublishedAsync(IReadOnlyCollection<Guid> tourIds)
    {
        if (tourIds.Count == 0)
        {
            return [];
        }

        return await _tourReadRepository.GetPublishedIdsAsync(tourIds);
    }
}
