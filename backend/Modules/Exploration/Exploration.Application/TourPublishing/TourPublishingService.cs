using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPublishing;

public sealed class TourPublishingService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentApi _paymentApi;

    public TourPublishingService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IPaymentApi paymentApi)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _paymentApi = paymentApi;
    }

    public async Task PublishAsync(Guid tourId, Guid authorId, PriceDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        var price = new Price(dto.IsFree, dto.Amount);

        tour.Publish(price);
        await _paymentApi.ListProductAsync(tour.Id, tour.Name, authorId, price.Amount);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
