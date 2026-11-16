using Exploration.Application.PositionSimulator;
using Exploration.Application.Tours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/positions")]
[Authorize(Roles = "explorer")]
public sealed class PositionSimulatorController : ControllerBase
{
    private readonly PositionSimulatorService _simulatorService;
    private readonly PositionSimulatorQueries _simulatorQueries;

    public PositionSimulatorController(
        PositionSimulatorService simulatorService,
        PositionSimulatorQueries simulatorQueries)
    {
        _simulatorService = simulatorService;
        _simulatorQueries = simulatorQueries;
    }

    [HttpPut("mine")]
    public async Task<ActionResult> MarkPosition(LocationDto dto)
    {
        await _simulatorService.MarkPositionAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<TouristPositionDto>> GetMine()
    {
        return await _simulatorQueries.GetMyPositionAsync(User.GetUserId());
    }
}
