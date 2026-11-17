using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourPreviewReadRepository : ITourPreviewReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreviewReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
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
}
