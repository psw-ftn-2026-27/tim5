using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Reversing;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/tourists")]
[Authorize(Roles = "administrator")]
public sealed class ReversingController : ControllerBase
{
    private readonly ReversingService _reversingService;
    private readonly ReversingQueries _reversingQueries;

    public ReversingController(ReversingService reversingService, ReversingQueries reversingQueries)
    {
        _reversingService = reversingService;
        _reversingQueries = reversingQueries;
    }

    [HttpGet("{touristId:guid}/deposits")]
    public async Task<ActionResult<TouristDepositsDto>> GetTouristDeposits(Guid touristId)
    {
        return await _reversingQueries.GetTouristDepositsAsync(touristId);
    }

    [HttpPost("{touristId:guid}/deposits/{depositId:guid}/reversal")]
    public async Task<ActionResult> Reverse(Guid touristId, Guid depositId)
    {
        await _reversingService.ReverseAsync(touristId, depositId, User.GetUserId());
        return NoContent();
    }
}
