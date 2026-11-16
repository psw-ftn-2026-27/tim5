using Exploration.Application.UploadedImages;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.UploadedImages;

internal sealed class UploadedImageReadRepository : IUploadedImageReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public UploadedImageReadRepository(ExplorationDbContext dbContext)
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
