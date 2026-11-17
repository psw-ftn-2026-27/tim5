namespace Exploration.Application.TourBrowsing;

public interface ITourPreviewReadRepository
{
    Task<TourPreviewDto?> GetPublishedPreviewAsync(Guid tourId);
}
