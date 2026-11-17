using Exploration.Application.TourBrowsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize]
public sealed class TourBrowsingController : ControllerBase
{
    private readonly TourBrowsingQueries _browsingQueries;

    public TourBrowsingController(TourBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<PublishedTourDto>>> GetPublished(
        [FromQuery] TourSearchCriteriaDto criteria,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetPublishedAsync(criteria, page, pageSize);
    }

    [HttpGet("published/{id:guid}")]
    public async Task<ActionResult<TourPreviewDto>> GetPublishedPreview(Guid id)
    {
        return await _browsingQueries.GetPublishedTourPreviewAsync(id);
    }
}
