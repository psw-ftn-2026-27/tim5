using Exploration.Application.TourAuthoring;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourAuthoringReadRepository : ITourAuthoringReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourAuthoringReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return TourDtoProjection.ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
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
}
