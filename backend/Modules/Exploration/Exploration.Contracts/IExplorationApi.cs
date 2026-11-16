namespace Exploration.Contracts;

public interface IExplorationApi
{
    Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId);
}
