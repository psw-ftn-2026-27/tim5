using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<TourWithKeyPointsDto?> GetOwnedWithKeyPointsAsync(Guid tourId, Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize);
}
