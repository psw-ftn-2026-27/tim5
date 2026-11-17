using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardReading;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardReadingController : ControllerBase
{
    private readonly GiftCardReadingQueries _readingQueries;

    public GiftCardReadingController(GiftCardReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet("sent")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetSent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _readingQueries.GetSentGiftCardsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("received")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetReceived(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _readingQueries.GetReceivedGiftCardsAsync(User.GetUserId(), page, pageSize);
    }
}
