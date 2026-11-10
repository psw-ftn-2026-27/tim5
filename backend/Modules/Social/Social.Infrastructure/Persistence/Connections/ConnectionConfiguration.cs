using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Connections;

namespace Social.Infrastructure.Persistence.Connections;

internal static class ConnectionConfiguration
{
    public static void ConfigureConnections(this ModelBuilder modelBuilder)
    {
        ConfigureConnection(modelBuilder.Entity<Connection>());
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
    }

    private static void ConfigureConnection(EntityTypeBuilder<Connection> builder)
    {
        builder.HasMany(connection => connection.FollowRequests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(connection => connection.FollowRequests).AutoInclude();
    }

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
    }
}
