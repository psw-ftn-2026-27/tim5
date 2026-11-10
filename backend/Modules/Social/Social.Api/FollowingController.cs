using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.Following;

namespace Social.Api;

[ApiController]
[Route("api/social/following")]
[Authorize(Roles = "explorer")]
public sealed class FollowingController : ControllerBase
{
    private readonly FollowingService _followingService;
    private readonly FollowingQueries _followingQueries;

    public FollowingController(FollowingService followingService, FollowingQueries followingQueries)
    {
        _followingService = followingService;
        _followingQueries = followingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<FollowingDto>>> GetFollowing()
    {
        return await _followingQueries.GetFollowingAsync(User.GetUserId());
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> SearchUsers([FromQuery] string username)
    {
        return await _followingQueries.SearchUsersAsync(username);
    }

    [HttpPost("{followeeId:guid}")]
    public async Task<ActionResult> RequestFollow(Guid followeeId)
    {
        await _followingService.RequestFollowAsync(User.GetUserId(), followeeId);
        return NoContent();
    }

    [HttpDelete("{followeeId:guid}/request")]
    public async Task<ActionResult> WithdrawRequest(Guid followeeId)
    {
        await _followingService.WithdrawRequestAsync(User.GetUserId(), followeeId);
        return NoContent();
    }

    [HttpDelete("{followeeId:guid}")]
    public async Task<ActionResult> Unfollow(Guid followeeId)
    {
        await _followingService.UnfollowAsync(User.GetUserId(), followeeId);
        return NoContent();
    }
}
