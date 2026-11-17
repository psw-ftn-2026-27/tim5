using Games.Application.UploadedImages;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.UploadedImages;

internal sealed class UploadedImageReadRepository : IUploadedImageReadRepository
{
    private readonly GamesDbContext _dbContext;

    public UploadedImageReadRepository(GamesDbContext dbContext)
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
