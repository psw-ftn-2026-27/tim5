using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourSearchCriteriaDto(
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    List<TransportMode>? Transports,
    TourSortOrder Sort = TourSortOrder.Newest);
