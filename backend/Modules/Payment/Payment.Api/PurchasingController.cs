using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Purchasing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/purchases")]
[Authorize(Roles = "explorer")]
public sealed class PurchasingController : ControllerBase
{
    private readonly PurchasingQueries _purchasingQueries;

    public PurchasingController(PurchasingQueries purchasingQueries)
    {
        _purchasingQueries = purchasingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<PurchasedTourDto>>> GetMyPurchases()
    {
        return await _purchasingQueries.GetMyPurchasesAsync(User.GetUserId());
    }
}
