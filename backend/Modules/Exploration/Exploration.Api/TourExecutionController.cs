using Exploration.Application.TourExecution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/executions")]
[Authorize(Roles = "explorer")]
public sealed class TourExecutionController : ControllerBase
{
    private readonly TourExecutionService _executionService;
    private readonly TourExecutionQueries _executionQueries;

    public TourExecutionController(
        TourExecutionService executionService,
        TourExecutionQueries executionQueries)
    {
        _executionService = executionService;
        _executionQueries = executionQueries;
    }

    [HttpPost("tours/{tourId:guid}")]
    public async Task<ActionResult> Start(Guid tourId)
    {
        await _executionService.StartAsync(User.GetUserId(), tourId);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<ActiveTourExecutionDto>> GetActive()
    {
        return await _executionQueries.GetActiveAsync(User.GetUserId());
    }

    [HttpPost("active/abandon")]
    public async Task<ActionResult> Abandon()
    {
        await _executionService.AbandonAsync(User.GetUserId());
        return NoContent();
    }
}
