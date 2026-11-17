using Exploration.Application.Tours;
using Payment.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourSearchReadRepository _tourSearchReadRepository;
    private readonly ITourPreviewReadRepository _tourPreviewReadRepository;
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingQueries(
        ITourSearchReadRepository tourSearchReadRepository,
        ITourPreviewReadRepository tourPreviewReadRepository,
        IPaymentApi paymentApi)
    {
        _tourSearchReadRepository = tourSearchReadRepository;
        _tourPreviewReadRepository = tourPreviewReadRepository;
        _paymentApi = paymentApi;
    }

    public async Task<PageResult<PublishedTourDto>> GetPublishedAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        if (!SearchesByPrice(criteria))
        {
            return await GetPageThenPricesAsync(criteria, page, pageSize);
        }
        return await GetPricesThenPageAsync(criteria, page, pageSize);
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

    // Nothing in the search depends on price, so prices are needed only for the shown page.
    private async Task<PageResult<PublishedTourDto>> GetPageThenPricesAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        var tours = await _tourSearchReadRepository.GetPublishedAsync(criteria, page, pageSize);
        var prices = await _paymentApi.GetTourPricesAsync(tours.Items.Select(tour => tour.Id).ToList());

        var items = tours.Items.Select(tour => ToPublishedTour(tour, prices)).ToList();
        return new PageResult<PublishedTourDto>(items, tours.TotalCount);
    }

    // Price filtering and sorting cover every matching tour, so the page is cut only after them.
    private async Task<PageResult<PublishedTourDto>> GetPricesThenPageAsync(TourSearchCriteriaDto criteria, int page, int pageSize)
    {
        var tourIds = await _tourSearchReadRepository.GetPublishedIdsAsync(criteria);
        var prices = await _paymentApi.GetTourPricesAsync(tourIds);

        var matchingIds = SortByPrice(FilterByPrice(tourIds, prices, criteria), prices, criteria.Sort);
        var pageIds = matchingIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var tours = await _tourSearchReadRepository.GetByIdsAsync(pageIds);

        var items = pageIds
            .Select(id => ToPublishedTour(tours.Single(tour => tour.Id == id), prices))
            .ToList();
        return new PageResult<PublishedTourDto>(items, matchingIds.Count);
    }

    private static bool SearchesByPrice(TourSearchCriteriaDto criteria)
    {
        return criteria.MinPrice is not null
            || criteria.MaxPrice is not null
            || criteria.Sort is TourSortOrder.PriceAscending or TourSortOrder.PriceDescending;
    }

    // With a price range chosen, a tour without a price is left out; bounds are inclusive.
    private static List<Guid> FilterByPrice(List<Guid> tourIds, IReadOnlyDictionary<Guid, int> prices, TourSearchCriteriaDto criteria)
    {
        if (criteria.MinPrice is null && criteria.MaxPrice is null)
        {
            return tourIds;
        }

        return tourIds
            .Where(id => prices.ContainsKey(id))
            .Where(id => criteria.MinPrice is null || prices[id] >= criteria.MinPrice)
            .Where(id => criteria.MaxPrice is null || prices[id] <= criteria.MaxPrice)
            .ToList();
    }

    // Tours without a price go last in both directions. OrderBy is stable, so tours of the
    // same price keep the order they arrived in, newest published first.
    private static List<Guid> SortByPrice(List<Guid> tourIds, IReadOnlyDictionary<Guid, int> prices, TourSortOrder sort)
    {
        var priced = tourIds.Where(id => prices.ContainsKey(id));
        var unpriced = tourIds.Where(id => !prices.ContainsKey(id));

        return sort switch
        {
            TourSortOrder.PriceAscending => priced.OrderBy(id => prices[id]).Concat(unpriced).ToList(),
            TourSortOrder.PriceDescending => priced.OrderByDescending(id => prices[id]).Concat(unpriced).ToList(),
            _ => tourIds
        };
    }

    private static PublishedTourDto ToPublishedTour(TourDto tour, IReadOnlyDictionary<Guid, int> prices)
    {
        int? price = prices.TryGetValue(tour.Id, out var amount) ? amount : null;

        return new PublishedTourDto(
            tour.Id,
            tour.AuthorId,
            tour.Name,
            tour.Description,
            tour.Difficulty,
            tour.Tags,
            tour.Status,
            tour.PublishedAt,
            tour.ArchivedAt,
            tour.RecommendedGroupSize,
            tour.TransportTimes,
            price);
    }
}
