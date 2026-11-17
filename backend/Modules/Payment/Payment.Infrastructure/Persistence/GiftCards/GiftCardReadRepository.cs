using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCardReading;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public GiftCardReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PageResult<GiftCardReadItemDto>> GetSentAsync(Guid senderId, int page, int pageSize)
    {
        return GetPageAsync(
            _dbContext.GiftCards.Where(giftCard => giftCard.SenderId == senderId),
            sent: true,
            page,
            pageSize);
    }

    public Task<PageResult<GiftCardReadItemDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize)
    {
        return GetPageAsync(
            _dbContext.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId),
            sent: false,
            page,
            pageSize);
    }

    private async Task<PageResult<GiftCardReadItemDto>> GetPageAsync(
        IQueryable<GiftCard> giftCards,
        bool sent,
        int page,
        int pageSize)
    {
        var totalCount = await giftCards.CountAsync();
        var items = await giftCards
            .OrderByDescending(giftCard => giftCard.CreatedAt)
            .ThenByDescending(giftCard => giftCard.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(giftCard => new GiftCardReadItemDto(
                giftCard.Id,
                sent ? giftCard.RecipientId : giftCard.SenderId,
                giftCard.Amount.Value,
                giftCard.Message,
                giftCard.CreatedAt,
                giftCard.ExpiresAt,
                giftCard.RedeemedAt))
            .ToListAsync();

        return new PageResult<GiftCardReadItemDto>(items, totalCount);
    }
}
