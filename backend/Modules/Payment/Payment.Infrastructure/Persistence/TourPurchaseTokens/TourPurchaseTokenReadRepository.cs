using Microsoft.EntityFrameworkCore;
using Payment.Application.Purchasing;
using Payment.Application.TourPurchaseTokens;
using Payment.Domain.Wallets;

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

    public Task<List<PurchasedTourDto>> GetPurchasedToursAsync(Guid touristId)
    {
        var purchaseTransactions = _dbContext.Wallets
            .Where(wallet => wallet.TouristId == touristId)
            .SelectMany(wallet => wallet.Transactions)
            .Where(transaction => transaction.Type == TransactionType.Purchase);

        return _dbContext.TourPurchaseTokens
            .Where(token => token.TouristId == touristId)
            .Join(
                _dbContext.Products,
                token => token.EntityId,
                product => product.EntityId,
                (token, product) => new { Token = token, Product = product })
            .GroupJoin(
                purchaseTransactions,
                tokenProduct => (Guid?)tokenProduct.Token.EntityId,
                transaction => transaction.EntityId,
                (tokenProduct, transactions) => new { tokenProduct.Token, tokenProduct.Product, Transactions = transactions })
            .SelectMany(
                tokenProduct => tokenProduct.Transactions.DefaultIfEmpty(),
                (tokenProduct, transaction) => new
                {
                    tokenProduct.Token.EntityId,
                    Name = transaction == null ? tokenProduct.Product.Name : transaction.Name!,
                    tokenProduct.Token.PurchasedAt
                })
            .OrderByDescending(purchase => purchase.PurchasedAt)
            .Select(purchase => new PurchasedTourDto(purchase.EntityId, purchase.Name, purchase.PurchasedAt))
            .ToListAsync();
    }
}
