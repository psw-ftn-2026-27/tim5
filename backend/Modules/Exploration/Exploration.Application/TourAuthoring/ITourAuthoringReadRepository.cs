using Exploration.Application.Tours;

namespace Exploration.Application.TourAuthoring;

public interface ITourAuthoringReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<TourWithKeyPointsDto?> GetOwnedWithKeyPointsAsync(Guid tourId, Guid authorId);
}
