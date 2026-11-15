using Games.Application.ChallengeOverview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/overview")]
[Authorize(Roles = "administrator")]
public sealed class ChallengeOverviewController : ControllerBase
{
    private readonly ChallengeOverviewQueries _overviewQueries;

    public ChallengeOverviewController(ChallengeOverviewQueries overviewQueries)
    {
        _overviewQueries = overviewQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<ChallengeOverviewItemDto>>> GetAll([FromQuery] ChallengeOverviewFilterDto filter)
    {
        return await _overviewQueries.GetAllAsync(filter);
    }
}
