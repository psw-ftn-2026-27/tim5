using Games.Application.LocationChallengeActivation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeActivationController : ControllerBase
{
    private readonly LocationChallengeActivationService _activationService;
    private readonly LocationChallengeActivationQueries _activationQueries;

    public LocationChallengeActivationController(
        LocationChallengeActivationService activationService,
        LocationChallengeActivationQueries activationQueries)
    {
        _activationService = activationService;
        _activationQueries = activationQueries;
    }

    [HttpPost("{id:guid}/activation")]
    public async Task<ActionResult> Activate(Guid id)
    {
        await _activationService.ActivateAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/activation/location-check")]
    public async Task<ActionResult> CheckLocation(Guid id)
    {
        await _activationService.CheckLocationAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpGet("{id:guid}/activation")]
    public async Task<ActionResult<ActiveLocationChallengeDto>> GetActive(Guid id)
    {
        return await _activationQueries.GetActiveAsync(User.GetUserId(), id);
    }
}
