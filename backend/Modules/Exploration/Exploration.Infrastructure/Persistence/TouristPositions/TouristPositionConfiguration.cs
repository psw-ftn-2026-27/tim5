using Exploration.Domain.TouristPositions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TouristPositions;

internal static class TouristPositionConfiguration
{
    public static void ConfigureTouristPositions(this ModelBuilder modelBuilder)
    {
        ConfigureTouristPosition(modelBuilder.Entity<TouristPosition>());
    }

    private static void ConfigureTouristPosition(EntityTypeBuilder<TouristPosition> builder)
    {
        builder.HasIndex(position => position.UserId).IsUnique();

        builder.OwnsOne(position => position.Location);
        builder.Navigation(position => position.Location).IsRequired();
    }
}
