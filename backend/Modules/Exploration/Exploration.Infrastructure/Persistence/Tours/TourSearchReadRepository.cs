using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourSearchReadRepository : ITourSearchReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourSearchReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
        var matching = Sort(Filter(published, criteria), criteria.Sort);

        var totalCount = await matching.CountAsync();
        var items = await TourDtoProjection.ProjectToDtos(matching)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public Task<List<Guid>> GetPublishedIdsAsync(TourSearchCriteriaDto criteria)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);

        return Sort(Filter(published, criteria), criteria.Sort)
            .Select(tour => tour.Id)
            .ToListAsync();
    }

    public Task<List<TourDto>> GetByIdsAsync(IReadOnlyCollection<Guid> tourIds)
    {
        var tours = _dbContext.Tours.Where(tour => tourIds.Contains(tour.Id));

        return TourDtoProjection.ProjectToDtos(tours).ToListAsync();
    }

    private static IQueryable<Tour> Filter(IQueryable<Tour> tours, TourSearchCriteriaDto criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            var name = criteria.Name.Trim().ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (criteria.Difficulties is { Count: > 0 })
        {
            var difficulties = criteria.Difficulties;
            tours = tours.Where(tour => difficulties.Contains(tour.Difficulty));
        }
        if (criteria.Tags is { Count: > 0 })
        {
            var tags = criteria.Tags.Select(tag => tag.Trim().ToLower()).ToList();
            tours = tours.Where(tour => tour.Tags.Any(tag => tags.Contains(tag.ToLower())));
        }
        if (criteria.Transports is { Count: > 0 })
        {
            var transports = criteria.Transports;
            tours = tours.Where(tour => tour.TransportTimes.Any(time => transports.Contains(time.Transport)));
        }
        return tours;
    }

    private static IQueryable<Tour> Sort(IQueryable<Tour> tours, TourSortOrder sort)
    {
        // Difficulty is stored as text, so the order Easy < Moderate < Hard is spelled out here.
        // Price sorts fall through to newest first; the price order is applied in TourBrowsingQueries.
        return sort switch
        {
            TourSortOrder.DifficultyAscending => tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            TourSortOrder.DifficultyDescending => tours
                .OrderByDescending(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            _ => tours.OrderByDescending(tour => tour.PublishedAt)
        };
    }
}
