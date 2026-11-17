namespace Exploration.Contracts;

public interface IExplorationApi
{
    Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId);
    Task<IReadOnlyCollection<Guid>> GetPublishedAsync(IReadOnlyCollection<Guid> tourIds);
}
