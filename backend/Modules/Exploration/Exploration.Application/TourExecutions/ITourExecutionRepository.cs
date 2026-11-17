namespace Exploration.Application.TourExecutions;

// The aggregate's name is qualified because, inside Exploration.Application, the simple name
// TourExecution means the use-case group's namespace.
public interface ITourExecutionRepository
{
    Task<Domain.TourExecutions.TourExecution?> GetActiveByTouristAsync(Guid touristId);
    void Add(Domain.TourExecutions.TourExecution execution);
}
