using Exploration.Application.TourAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public Task<TourWithKeyPointsDto?> GetOwnedWithKeyPointsAsync(Guid tourId, Guid authorId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new TourWithKeyPointsDto(
                tour.Id,
                tour.Name,
                tour.Status,
                tour.LengthInKm,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageId,
                        ImageAddress.For(keyPoint.ImageId),
                        keyPoint.Secret,
                        keyPoint.IsPublic,
                        keyPoint.Order,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        var published = _dbContext.Tours.Where(tour => tour.Status == TourStatus.Published);
        var matching = Sort(Filter(published, criteria), criteria.Sort);

        var totalCount = await matching.CountAsync();
        var items = await ProjectToDtos(matching)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public Task<TourPreviewDto?> GetPublishedPreviewAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.Status == TourStatus.Published)
            .Select(tour => new TourPreviewDto(
                tour.Id,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.LengthInKm,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsPublic)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new PublicKeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        ImageAddress.For(keyPoint.ImageId),
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
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

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                tour.RecommendedGroupSize == null
                    ? null
                    : new GroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max),
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList()));
    }
}
