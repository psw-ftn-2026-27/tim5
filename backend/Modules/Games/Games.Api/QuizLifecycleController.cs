using Games.Application.QuizLifecycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizLifecycleController : ControllerBase
{
    private readonly QuizLifecycleService _lifecycleService;

    public QuizLifecycleController(QuizLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _lifecycleService.DeleteAsync(id);
        return NoContent();
    }
}
