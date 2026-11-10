using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.FollowerManagement;

namespace Social.Api;

[ApiController]
[Route("api/social/followers")]
[Authorize(Roles = "explorer")]
public sealed class FollowerManagementController : ControllerBase
{
    private readonly FollowerManagementService _followerManagementService;
    private readonly FollowerManagementQueries _followerManagementQueries;

    public FollowerManagementController(
        FollowerManagementService followerManagementService,
        FollowerManagementQueries followerManagementQueries)
    {
        _followerManagementService = followerManagementService;
        _followerManagementQueries = followerManagementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<FollowersDto>> GetFollowers()
    {
        return await _followerManagementQueries.GetFollowersAsync(User.GetUserId());
    }

    [HttpPost("{followerId:guid}/accept")]
    public async Task<ActionResult> AcceptRequest(Guid followerId)
    {
        await _followerManagementService.AcceptRequestAsync(User.GetUserId(), followerId);
        return NoContent();
    }

    [HttpPost("{followerId:guid}/reject")]
    public async Task<ActionResult> RejectRequest(Guid followerId)
    {
        await _followerManagementService.RejectRequestAsync(User.GetUserId(), followerId);
        return NoContent();
    }

    [HttpDelete("{followerId:guid}")]
    public async Task<ActionResult> RemoveFollower(Guid followerId)
    {
        await _followerManagementService.RemoveFollowerAsync(User.GetUserId(), followerId);
        return NoContent();
    }
}
