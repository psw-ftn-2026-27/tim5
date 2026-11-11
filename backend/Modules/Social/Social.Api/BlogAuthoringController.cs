using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain.Images;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;

namespace Social.Api;

[ApiController]
[Route("api/social/blogs")]
[Authorize(Roles = "explorer")]
public sealed class BlogAuthoringController : ControllerBase
{
    private readonly BlogAuthoringService _authoringService;
    private readonly BlogAuthoringQueries _authoringQueries;

    public BlogAuthoringController(
        BlogAuthoringService authoringService,
        BlogAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<BlogDto>> Create(CreateBlogDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _authoringService.PublishAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<BlogDto>>> GetMine()
    {
        return await _authoringQueries.GetByAuthorAsync(User.GetUserId());
    }

    // The limit sits slightly above the image size limit, so the framework cuts off an
    // oversized body before it is read, while the domain still decides on the exact size.
    [HttpPost("/api/social/images")]
    [RequestSizeLimit(ImageFile.MaxSizeInBytes + 64 * 1024)]
    public async Task<ActionResult<UploadedImageDto>> UploadImage(IFormFile file)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        return await _authoringService.UploadImageAsync(User.GetUserId(), content.ToArray());
    }

    [HttpDelete("/api/social/images/{id:guid}")]
    public async Task<ActionResult> DeleteImage(Guid id)
    {
        await _authoringService.DeleteImageAsync(id, User.GetUserId());
        return NoContent();
    }
}
