using Games.Application.LocationChallengeBrowsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "explorer")]
public sealed class LocationChallengeBrowsingController : ControllerBase
{
    private readonly LocationChallengeBrowsingQueries _browsingQueries;

    public LocationChallengeBrowsingController(LocationChallengeBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<NearbyLocationChallengesDto>> GetNearby()
    {
        return await _browsingQueries.GetNearbyAsync(User.GetUserId());
    }
}
