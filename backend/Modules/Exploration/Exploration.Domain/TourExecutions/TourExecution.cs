using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

public sealed class TourExecution : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public Location StartPosition { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public DateTime LastActivity { get; private set; }

    private TourExecution()
    {
        StartPosition = null!;
    }

    // Created through TourExecutionStarter, which checks the rules spanning the tour and other executions.
    public TourExecution(Guid touristId, Guid tourId, Location startPosition, DateTime startedAt)
        : base(Guid.NewGuid())
    {
        if (startPosition is null)
        {
            throw new DomainException("A tour execution requires a start position.");
        }

        TouristId = touristId;
        TourId = tourId;
        StartPosition = startPosition;
        StartedAt = startedAt;
        LastActivity = startedAt;
        Status = TourExecutionStatus.Active;
    }

    public void Abandon(DateTime abandonedAt)
    {
        if (Status != TourExecutionStatus.Active)
        {
            throw new DomainException("Only an active tour execution can be abandoned.");
        }

        Status = TourExecutionStatus.Abandoned;
        EndedAt = abandonedAt;
    }
}
