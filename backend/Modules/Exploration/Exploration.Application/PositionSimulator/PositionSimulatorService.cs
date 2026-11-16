using Exploration.Application.TouristPositions;
using Exploration.Application.Tours;
using Exploration.Domain.TouristPositions;
using Exploration.Domain.Tours;

namespace Exploration.Application.PositionSimulator;

public sealed class PositionSimulatorService
{
    private readonly ITouristPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PositionSimulatorService(ITouristPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task MarkPositionAsync(Guid userId, LocationDto dto)
    {
        var location = new Location(dto.Latitude, dto.Longitude);
        var markedAt = DateTime.UtcNow;

        var position = await _positionRepository.GetByUserAsync(userId);
        if (position is null)
        {
            _positionRepository.Add(new TouristPosition(userId, location, markedAt));
        }
        else
        {
            position.MarkAt(location, markedAt);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
