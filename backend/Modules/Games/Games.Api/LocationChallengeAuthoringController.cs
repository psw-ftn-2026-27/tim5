using Games.Application.LocationChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeAuthoringController : ControllerBase
{
    private readonly LocationChallengeAuthoringService _authoringService;
    private readonly LocationChallengeAuthoringQueries _authoringQueries;

    public LocationChallengeAuthoringController(
        LocationChallengeAuthoringService authoringService,
        LocationChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationChallengeSummaryDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationChallengeDto>> GetById(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<LocationChallengeDto>> Create(LocationChallengeInputDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, LocationChallengeInputDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }
}
