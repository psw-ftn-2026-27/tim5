using Games.Application.SocialChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeAuthoringController : ControllerBase
{
    private readonly SocialChallengeAuthoringService _authoringService;
    private readonly SocialChallengeAuthoringQueries _authoringQueries;

    public SocialChallengeAuthoringController(
        SocialChallengeAuthoringService authoringService,
        SocialChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<SocialChallengeDto>>> GetAll([FromQuery] SocialChallengeFilterDto filter)
    {
        return await _authoringQueries.GetAllAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<SocialChallengeDto>> Create(SocialChallengeInputDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, SocialChallengeInputDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }
}
