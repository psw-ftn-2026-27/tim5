using Games.Domain.LocationChallengeActivations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallengeActivations;

internal static class LocationChallengeActivationConfiguration
{
    public static void ConfigureLocationChallengeActivations(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallengeActivation(modelBuilder.Entity<LocationChallengeActivation>());
    }

    private static void ConfigureLocationChallengeActivation(EntityTypeBuilder<LocationChallengeActivation> builder)
    {
        builder.HasIndex(activation => new { activation.UserId, activation.ChallengeId }).IsUnique();

        builder.PrimitiveCollection(activation => activation.DiscoveredPointIds);

        builder.OwnsOne(activation => activation.Completion);
    }
}
