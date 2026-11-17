using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

public static class TourExecutionStarter
{
    public static TourExecution Start(Guid touristId, Tour tour, bool isPurchased, bool hasActiveExecution, Location startPosition, DateTime startedAt)
    {
        if (!isPurchased)
        {
            throw new DomainException("Only a purchased tour can be started.");
        }
        if (!tour.IsAvailableForExecution())
        {
            throw new DomainException("This tour is not available for execution.");
        }
        if (hasActiveExecution)
        {
            throw new DomainException("You already have an active tour.");
        }

        return new TourExecution(touristId, tour.Id, startPosition, startedAt);
    }
}
