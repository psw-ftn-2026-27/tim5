using Exploration.Application.TourExecutions;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecution;

public sealed class TourExecutionQueries
{
    private readonly ITourExecutionReadRepository _executionReadRepository;

    public TourExecutionQueries(ITourExecutionReadRepository executionReadRepository)
    {
        _executionReadRepository = executionReadRepository;
    }

    public async Task<ActiveTourExecutionDto> GetActiveAsync(Guid touristId)
    {
        var execution = await _executionReadRepository.GetActiveAsync(touristId);
        if (execution is null)
        {
            throw new NotFoundException("You have no active tour.");
        }

        return execution;
    }
}
