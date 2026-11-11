using Microsoft.EntityFrameworkCore;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.Clubs;
using Social.Domain.Connections;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.Connections;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Club> Clubs => Set<Club>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureConnections();
        builder.ConfigureClubs();
    }
}
