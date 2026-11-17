using Games.Application.HiddenPointAuthoring;
using Games.Application.UploadedImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain.Images;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class HiddenPointAuthoringController : ControllerBase
{
    private readonly HiddenPointAuthoringService _hiddenPointService;

    public HiddenPointAuthoringController(HiddenPointAuthoringService hiddenPointService)
    {
        _hiddenPointService = hiddenPointService;
    }

    [HttpPost("{id:guid}/hidden-points")]
    public async Task<ActionResult> AddHiddenPoint(Guid id, HiddenPointInputDto dto)
    {
        await _hiddenPointService.AddHiddenPointAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/hidden-points/{pointId:guid}")]
    public async Task<ActionResult> ChangeHiddenPoint(Guid id, Guid pointId, HiddenPointInputDto dto)
    {
        await _hiddenPointService.ChangeHiddenPointAsync(id, pointId, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/hidden-points/{pointId:guid}")]
    public async Task<ActionResult> RemoveHiddenPoint(Guid id, Guid pointId)
    {
        await _hiddenPointService.RemoveHiddenPointAsync(id, pointId);
        return NoContent();
    }

    // The limit sits slightly above the image size limit, so the framework cuts off an
    // oversized body before it is read, while the domain still decides on the exact size.
    [HttpPost("/api/games/images")]
    [RequestSizeLimit(ImageFile.MaxSizeInBytes + 64 * 1024)]
    public async Task<ActionResult<UploadedImageDto>> UploadImage(IFormFile file)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        return await _hiddenPointService.UploadImageAsync(User.GetUserId(), content.ToArray());
    }
}
