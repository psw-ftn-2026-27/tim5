using Payment.Application.Purchases;
using Payment.Domain.Purchases;

namespace Payment.Infrastructure.Persistence.Purchases;

internal sealed class PurchaseRepository : IPurchaseRepository
{
    private readonly PaymentDbContext _dbContext;

    public PurchaseRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Purchase purchase)
    {
        _dbContext.Purchases.Add(purchase);
    }
}
