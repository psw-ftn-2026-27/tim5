using Payment.Application.GiftCardReading;
using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<GiftCardReadItemDto>> GetSentAsync(Guid senderId, int page, int pageSize);
    Task<PageResult<GiftCardReadItemDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize);
}
