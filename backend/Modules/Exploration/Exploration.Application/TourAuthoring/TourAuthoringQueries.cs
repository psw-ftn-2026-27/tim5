using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourAuthoring;

public sealed class TourAuthoringQueries
{
    private readonly ITourAuthoringReadRepository _tourAuthoringReadRepository;

    public TourAuthoringQueries(ITourAuthoringReadRepository tourAuthoringReadRepository)
    {
        _tourAuthoringReadRepository = tourAuthoringReadRepository;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return _tourAuthoringReadRepository.GetByAuthorAsync(authorId);
    }

    public async Task<TourWithKeyPointsDto> GetOwnedTourWithKeyPointsAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourAuthoringReadRepository.GetOwnedWithKeyPointsAsync(tourId, authorId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        return tour;
    }
}
