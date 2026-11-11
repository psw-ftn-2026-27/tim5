using AutoMapper;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Social.Application.Blogs;
using Social.Application.UploadedImages;
using Social.Domain.Blogs;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUploadedImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BlogAuthoringService(
        IBlogRepository blogRepository,
        IUploadedImageRepository imageRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _blogRepository = blogRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BlogDto> CreateAsync(Guid authorId, CreateBlogDto dto)
    {
        var imageIds = dto.ImageIds ?? [];
        var images = new List<UploadedImage>();
        foreach (var imageId in imageIds)
        {
            images.Add(await GetImageAsync(imageId));
        }

        var blog = new Blog(authorId, dto.Title, dto.Description, imageIds);
        foreach (var image in images)
        {
            image.LinkTo(authorId);
        }

        _blogRepository.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogDto>(blog);
    }

    public async Task PublishAsync(Guid blogId, Guid authorId)
    {
        var blog = await GetOwnedBlogAsync(blogId, authorId);

        blog.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UploadedImageDto> UploadImageAsync(Guid authorId, byte[] content)
    {
        var file = new ImageFile(content);
        var unlinkedImageCount = await _imageRepository.CountUnlinkedByOwnerAsync(authorId);
        var image = UploadQuota.CreateImage(authorId, unlinkedImageCount, file);

        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        return new UploadedImageDto(image.Id, ImageAddress.For(image.Id));
    }

    public async Task DeleteImageAsync(Guid imageId, Guid authorId)
    {
        var image = await GetImageAsync(imageId);

        image.Delete(authorId);
        _imageRepository.Remove(image);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Blog> GetOwnedBlogAsync(Guid blogId, Guid authorId)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null || blog.AuthorId != authorId)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }
        return blog;
    }

    private async Task<UploadedImage> GetImageAsync(Guid imageId)
    {
        var image = await _imageRepository.GetByIdAsync(imageId);
        if (image is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }
        return image;
    }
}
