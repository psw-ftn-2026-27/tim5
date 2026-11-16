using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.UploadedImages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain.Images;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeyPointAuthoringController : ControllerBase
{
    private readonly KeyPointAuthoringService _keyPointService;

    public KeyPointAuthoringController(KeyPointAuthoringService keyPointService)
    {
        _keyPointService = keyPointService;
    }

    [HttpPost("{id:guid}/key-points")]
    public async Task<ActionResult> AddKeyPoint(Guid id, AddKeyPointDto dto)
    {
        await _keyPointService.AddKeyPointAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> ChangeKeyPoint(Guid id, Guid keyPointId, ChangeKeyPointDto dto)
    {
        await _keyPointService.ChangeKeyPointAsync(id, keyPointId, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> RemoveKeyPoint(Guid id, Guid keyPointId)
    {
        await _keyPointService.RemoveKeyPointAsync(id, keyPointId, User.GetUserId());
        return NoContent();
    }

    [HttpPut("{id:guid}/length")]
    public async Task<ActionResult> RecordLength(Guid id, TourLengthDto dto)
    {
        await _keyPointService.RecordLengthAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    // The limit sits slightly above the image size limit, so the framework cuts off an
    // oversized body before it is read, while the domain still decides on the exact size.
    [HttpPost("/api/exploration/images")]
    [RequestSizeLimit(ImageFile.MaxSizeInBytes + 64 * 1024)]
    public async Task<ActionResult<UploadedImageDto>> UploadImage(IFormFile file)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        return await _keyPointService.UploadImageAsync(User.GetUserId(), content.ToArray());
    }
}
