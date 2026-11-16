using Exploration.Application;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Images;
using Shared.Infrastructure;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options) { }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<UploadedImage> UploadedImages => Set<UploadedImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ConfigureTours();
        builder.ConfigureUploadedImages();
    }
}
