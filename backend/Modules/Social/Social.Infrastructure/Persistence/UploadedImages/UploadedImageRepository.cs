using Microsoft.EntityFrameworkCore;
using Shared.Domain.Images;
using Social.Application.UploadedImages;

namespace Social.Infrastructure.Persistence.UploadedImages;

internal sealed class UploadedImageRepository : IUploadedImageRepository
{
    private readonly SocialDbContext _dbContext;

    public UploadedImageRepository(SocialDbContext dbContext)
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

    public void Remove(UploadedImage image)
    {
        _dbContext.UploadedImages.Remove(image);
    }
}
