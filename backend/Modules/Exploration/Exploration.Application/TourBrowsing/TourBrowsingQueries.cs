using Exploration.Application.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourSearchReadRepository _tourSearchReadRepository;
    private readonly ITourPreviewReadRepository _tourPreviewReadRepository;

    public TourBrowsingQueries(
        ITourSearchReadRepository tourSearchReadRepository,
        ITourPreviewReadRepository tourPreviewReadRepository)
    {
        _tourSearchReadRepository = tourSearchReadRepository;
        _tourPreviewReadRepository = tourPreviewReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourSearchReadRepository.GetPublishedAsync(criteria, page, pageSize);
    }

    public async Task<TourPreviewDto> GetPublishedTourPreviewAsync(Guid tourId)
    {
        var preview = await _tourPreviewReadRepository.GetPublishedPreviewAsync(tourId);
        if (preview is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return preview;
    }
}
