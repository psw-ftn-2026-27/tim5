using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubAuthoring;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubAuthoringController : ControllerBase
{
    private readonly ClubAuthoringService _authoringService;

    public ClubAuthoringController(ClubAuthoringService authoringService)
    {
        _authoringService = authoringService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateClubDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateClubDto dto)
    {
        await _authoringService.UpdateAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _authoringService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }
}
