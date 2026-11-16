using Exploration.Application.UploadedImages;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Images;

namespace Exploration.Infrastructure.Persistence.UploadedImages;

internal sealed class UploadedImageRepository : IUploadedImageRepository
{
    private readonly ExplorationDbContext _dbContext;

    public UploadedImageRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UploadedImage?> GetByIdAsync(Guid id)
    {
        return _dbContext.UploadedImages.FirstOrDefaultAsync(image => image.Id == id);
    }

    public Task<int> CountUnlinkedByOwnerAsync(Guid ownerId)
    {
        return _dbContext.UploadedImages.CountAsync(image => image.OwnerId == ownerId && !image.IsLinked);
    }

    public void Add(UploadedImage image)
    {
        _dbContext.UploadedImages.Add(image);
    }
}
