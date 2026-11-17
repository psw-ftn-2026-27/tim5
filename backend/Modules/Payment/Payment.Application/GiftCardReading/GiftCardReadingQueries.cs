using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Application.GiftCardReading;

public sealed class GiftCardReadingQueries
{
    private readonly IGiftCardReadRepository _giftCardReadRepository;
    private readonly IIdentityApi _identityApi;

    public GiftCardReadingQueries(IGiftCardReadRepository giftCardReadRepository, IIdentityApi identityApi)
    {
        _giftCardReadRepository = giftCardReadRepository;
        _identityApi = identityApi;
    }

    public Task<PageResult<GiftCardDto>> GetSentGiftCardsAsync(Guid senderId, int page, int pageSize)
    {
        return GetGiftCardsAsync(senderId, page, pageSize, _giftCardReadRepository.GetSentAsync);
    }

    public Task<PageResult<GiftCardDto>> GetReceivedGiftCardsAsync(Guid recipientId, int page, int pageSize)
    {
        return GetGiftCardsAsync(recipientId, page, pageSize, _giftCardReadRepository.GetReceivedAsync);
    }

    private async Task<PageResult<GiftCardDto>> GetGiftCardsAsync(
        Guid userId,
        int page,
        int pageSize,
        Func<Guid, int, int, Task<PageResult<GiftCardReadItemDto>>> getPageAsync)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var pageResult = await getPageAsync(userId, page, pageSize);
        var otherUserIds = pageResult.Items.Select(item => item.OtherUserId).Distinct().ToList();
        var users = await _identityApi.GetByIdsAsync(otherUserIds);
        var usernamesById = users.ToDictionary(user => user.UserId, user => user.Username);
        var now = DateTime.UtcNow;

        var items = pageResult.Items.Select(item => new GiftCardDto(
            item.Id,
            usernamesById.GetValueOrDefault(item.OtherUserId),
            item.Amount,
            item.Message,
            item.CreatedAt,
            item.ExpiresAt,
            GiftCard.GetStatus(item.ExpiresAt, item.RedeemedAt, now))).ToList();

        return new PageResult<GiftCardDto>(items, pageResult.TotalCount);
    }
}
