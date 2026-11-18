using Exploration.Application.TourExecutions;
using Exploration.Application.TouristPositions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecution;

public sealed class TourExecutionService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly ITouristPositionReadRepository _positionReadRepository;
    private readonly IPaymentApi _paymentApi;
    private readonly IUnitOfWork _unitOfWork;

    public TourExecutionService(
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        ITouristPositionReadRepository positionReadRepository,
        IPaymentApi paymentApi,
        IUnitOfWork unitOfWork)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _positionReadRepository = positionReadRepository;
        _paymentApi = paymentApi;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid touristId, Guid tourId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        var position = await _positionReadRepository.GetByUserAsync(touristId);
        if (position is null)
        {
            throw new NotFoundException("Set your position in the position simulator before starting a tour.");
        }

        var isPurchased = await _paymentApi.HasPurchasedAsync(touristId, tourId);
        var hasActiveExecution = await _executionRepository.GetActiveByTouristAsync(touristId) is not null;
        var startPosition = new Location(position.Latitude, position.Longitude);

        var execution = TourExecutionStarter.Start(touristId, tour, isPurchased, hasActiveExecution, startPosition, DateTime.UtcNow);
        _executionRepository.Add(execution);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckPositionAsync(Guid touristId)
    {
        var execution = await _executionRepository.GetActiveByTouristAsync(touristId);
        if (execution is null)
        {
            throw new NotFoundException("You have no active tour.");
        }

        var tour = await _tourRepository.GetByIdAsync(execution.TourId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {execution.TourId} does not exist.");
        }

        var position = await _positionReadRepository.GetByUserAsync(touristId);
        if (position is null)
        {
            throw new NotFoundException("Set your position in the position simulator.");
        }

        TourProgress.CheckPosition(execution, tour, new Location(position.Latitude, position.Longitude), DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AbandonAsync(Guid touristId)
    {
        var execution = await _executionRepository.GetActiveByTouristAsync(touristId);
        if (execution is null)
        {
            throw new NotFoundException("You have no active tour.");
        }

        execution.Abandon(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
