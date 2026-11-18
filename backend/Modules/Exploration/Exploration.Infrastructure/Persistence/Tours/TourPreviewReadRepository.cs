using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Application.UploadedImages;
using Exploration.Domain.TourExecutions;
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

    // A secret is shown only for a key point the user completed in any of their executions of this tour.
    public Task<TourPreviewDto?> GetPreviewAsync(Guid tourId, Guid userId, bool isPurchased)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .Where(tour => tour.Status == TourStatus.Published || (isPurchased && tour.Status == TourStatus.Archived))
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
                    .ToList(),
                isPurchased,
                _dbContext.TourExecutions.Any(execution =>
                    execution.TouristId == userId && execution.Status == TourExecutionStatus.Active),
                tour.KeyPoints
                    .Where(keyPoint => _dbContext.TourExecutions.Any(execution =>
                        execution.TouristId == userId
                        && execution.TourId == tour.Id
                        && execution.CompletedKeyPoints.Any(completed => completed.KeyPointId == keyPoint.Id)))
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new CompletedKeyPointSecretDto(keyPoint.Id, keyPoint.Name, keyPoint.Secret))
                    .ToList(),
                _dbContext.TourExecutions.Any(execution =>
                    execution.TouristId == userId
                    && execution.TourId == tour.Id
                    && execution.Status == TourExecutionStatus.Completed)))
            .FirstOrDefaultAsync();
    }
}
