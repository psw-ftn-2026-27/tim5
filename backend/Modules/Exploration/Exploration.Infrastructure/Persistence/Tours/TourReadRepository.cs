using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Guid>> GetPublishedIdsAsync(IReadOnlyCollection<Guid> tourIds)
    {
        return _dbContext.Tours
            .Where(tour => tourIds.Contains(tour.Id) && tour.Status == TourStatus.Published)
            .Select(tour => tour.Id)
            .ToListAsync();
    }
}
