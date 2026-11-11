using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.UploadedImages;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IUploadedImageReadRepository _imageReadRepository;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository, IUploadedImageReadRepository imageReadRepository)
    {
        _blogReadRepository = blogReadRepository;
        _imageReadRepository = imageReadRepository;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id);
        if (blog is null || blog.Status != BlogStatus.Published)
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        return blog;
    }

    public Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetPublishedAsync(page, pageSize);
    }

    public async Task<ImageFileDto> GetImageFileAsync(Guid imageId)
    {
        var file = await _imageReadRepository.GetFileAsync(imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return file;
    }
}
