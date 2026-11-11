using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal static class ClubConfiguration
{
    public static void ConfigureClubs(this ModelBuilder modelBuilder)
    {
        ConfigureClub(modelBuilder.Entity<Club>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.OwnsOne(club => club.Name, name =>
        {
            name.Property(clubName => clubName.Value).HasMaxLength(200);
            name.Property(clubName => clubName.NormalizedValue).HasMaxLength(200);
        });
        builder.Navigation(club => club.Name).IsRequired();
    }
}
