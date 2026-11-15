using Games.Application.QuizAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizAuthoringController : ControllerBase
{
    private readonly QuizAuthoringService _authoringService;
    private readonly QuizAuthoringQueries _authoringQueries;

    public QuizAuthoringController(QuizAuthoringService authoringService, QuizAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuizSummaryDto>>> GetAll()
    {
        return await _authoringQueries.GetAllAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizDto>> GetById(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<QuizDto>> Create(QuizInputDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, QuizInputDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }
}
