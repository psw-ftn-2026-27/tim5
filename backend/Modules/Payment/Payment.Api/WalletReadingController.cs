using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletReading;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallets")]
[Authorize(Roles = "explorer")]
public sealed class WalletReadingController : ControllerBase
{
    private readonly WalletReadingQueries _readingQueries;

    public WalletReadingController(WalletReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<WalletDto>> GetMine()
    {
        return await _readingQueries.GetMineAsync(User.GetUserId());
    }
}
