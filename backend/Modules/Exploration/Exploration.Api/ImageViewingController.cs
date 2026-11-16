using Exploration.Application.ImageViewing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/images")]
public sealed class ImageViewingController : ControllerBase
{
    private readonly ImageViewingQueries _viewingQueries;

    public ImageViewingController(ImageViewingQueries viewingQueries)
    {
        _viewingQueries = viewingQueries;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetImageFile(Guid id)
    {
        var file = await _viewingQueries.GetImageFileAsync(id);
        return File(file.Content, file.ContentType);
    }
}
