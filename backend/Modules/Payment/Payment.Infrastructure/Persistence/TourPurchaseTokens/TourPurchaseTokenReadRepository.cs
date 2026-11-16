using Microsoft.EntityFrameworkCore;
using Payment.Application.TourPurchaseTokens;

namespace Payment.Infrastructure.Persistence.TourPurchaseTokens;

internal sealed class TourPurchaseTokenReadRepository : ITourPurchaseTokenReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public TourPurchaseTokenReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid touristId, Guid entityId)
    {
        return _dbContext.TourPurchaseTokens
            .AnyAsync(token => token.TouristId == touristId && token.EntityId == entityId);
    }
}
