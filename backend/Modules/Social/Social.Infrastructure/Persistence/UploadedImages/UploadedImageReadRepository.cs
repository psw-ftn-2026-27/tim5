using Microsoft.EntityFrameworkCore;
using Social.Application.UploadedImages;

namespace Social.Infrastructure.Persistence.UploadedImages;

internal sealed class UploadedImageReadRepository : IUploadedImageReadRepository
{
    private readonly SocialDbContext _dbContext;

    public UploadedImageReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ImageFileDto?> GetFileAsync(Guid id)
    {
        return _dbContext.UploadedImages
            .Where(image => image.Id == id)
            .Select(image => new ImageFileDto(image.File.Content, image.File.ContentType))
            .FirstOrDefaultAsync();
    }
}
