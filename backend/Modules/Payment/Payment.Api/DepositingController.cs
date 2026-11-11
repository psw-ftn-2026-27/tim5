using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Depositing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/tourists")]
[Authorize(Roles = "administrator")]
public sealed class DepositingController : ControllerBase
{
    private readonly DepositingService _depositingService;
    private readonly DepositingQueries _depositingQueries;

    public DepositingController(DepositingService depositingService, DepositingQueries depositingQueries)
    {
        _depositingService = depositingService;
        _depositingQueries = depositingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<TouristBalanceDto>>> GetTouristBalances([FromQuery] string username)
    {
        return await _depositingQueries.GetTouristBalancesAsync(username);
    }

    [HttpPost("{touristId:guid}/deposits")]
    public async Task<ActionResult> Deposit(Guid touristId, DepositDto dto)
    {
        await _depositingService.DepositAsync(touristId, User.GetUserId(), dto);
        return NoContent();
    }
}
