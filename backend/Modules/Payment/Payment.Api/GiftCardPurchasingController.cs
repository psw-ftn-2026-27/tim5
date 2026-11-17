using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardPurchasing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardPurchasingController : ControllerBase
{
    private readonly GiftCardPurchasingService _purchasingService;

    public GiftCardPurchasingController(GiftCardPurchasingService purchasingService)
    {
        _purchasingService = purchasingService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Purchase(PurchaseGiftCardDto dto)
    {
        var giftCardId = await _purchasingService.PurchaseGiftCardAsync(User.GetUserId(), dto);
        return Ok(giftCardId);
    }
}
