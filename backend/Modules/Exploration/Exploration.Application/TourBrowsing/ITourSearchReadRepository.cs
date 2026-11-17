using Exploration.Application.Tours;
using Shared.Domain;

namespace Exploration.Application.TourBrowsing;

public interface ITourSearchReadRepository
{
    Task<PageResult<TourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize);
}
