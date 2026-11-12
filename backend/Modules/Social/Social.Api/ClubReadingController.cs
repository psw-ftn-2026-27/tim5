using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;
using Social.Application.ClubReading;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubReadingController : ControllerBase
{
    private readonly ClubReadingQueries _readingQueries;

    public ClubReadingController(ClubReadingQueries readingQueries)
    {
        _readingQueries = readingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<ClubDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _readingQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubDto>> GetById(Guid id)
    {
        return await _readingQueries.GetByIdAsync(id);
    }
}
