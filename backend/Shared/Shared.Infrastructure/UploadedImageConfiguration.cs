using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Images;

namespace Shared.Infrastructure;

// Shared mapping for UploadedImage. Every module that uses images calls it from its own
// DbContext, so the table lands in that module's schema.
public static class UploadedImageConfiguration
{
    public static void ConfigureUploadedImages(this ModelBuilder modelBuilder)
    {
        ConfigureUploadedImage(modelBuilder.Entity<UploadedImage>());
    }

    private static void ConfigureUploadedImage(EntityTypeBuilder<UploadedImage> builder)
    {
        builder.ToTable("UploadedImages");
        builder.Property(image => image.StoredName).HasMaxLength(100);

        builder.OwnsOne(image => image.File, file =>
        {
            file.Property(imageFile => imageFile.ContentType).HasMaxLength(20);
        });
        builder.Navigation(image => image.File).IsRequired();
    }
}
