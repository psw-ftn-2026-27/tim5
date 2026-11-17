namespace Exploration.Application.TourBrowsing;

public interface ITourPreviewReadRepository
{
    Task<TourPreviewDto?> GetPreviewAsync(Guid tourId, Guid userId, bool isPurchased);
}
