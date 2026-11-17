namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<Guid>> GetPublishedIdsAsync(IReadOnlyCollection<Guid> tourIds);
}
