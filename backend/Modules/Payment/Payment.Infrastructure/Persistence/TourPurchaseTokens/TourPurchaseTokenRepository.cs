using Microsoft.EntityFrameworkCore;
using Payment.Application.TourPurchaseTokens;
using Payment.Domain.TourPurchaseTokens;

namespace Payment.Infrastructure.Persistence.TourPurchaseTokens;

internal sealed class TourPurchaseTokenRepository : ITourPurchaseTokenRepository
{
    private readonly PaymentDbContext _dbContext;

    public TourPurchaseTokenRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPurchaseToken?> GetByTouristAndEntityAsync(Guid touristId, Guid entityId)
    {
        return _dbContext.TourPurchaseTokens
            .FirstOrDefaultAsync(token => token.TouristId == touristId && token.EntityId == entityId);
    }

    public void Add(TourPurchaseToken token)
    {
        _dbContext.TourPurchaseTokens.Add(token);
    }
}
