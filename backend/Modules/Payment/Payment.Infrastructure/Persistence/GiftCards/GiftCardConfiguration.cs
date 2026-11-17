using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal static class GiftCardConfiguration
{
    public static void ConfigureGiftCards(this ModelBuilder modelBuilder)
    {
        ConfigureGiftCard(modelBuilder.Entity<GiftCard>());
    }

    private static void ConfigureGiftCard(EntityTypeBuilder<GiftCard> builder)
    {
        builder.HasIndex(giftCard => giftCard.SenderId);
        builder.HasIndex(giftCard => giftCard.RecipientId);
        builder.ComplexProperty(giftCard => giftCard.Amount, amount =>
        {
            amount.Property(value => value.Value).HasColumnName("Amount");
        });
        builder.Property(giftCard => giftCard.Message).HasMaxLength(200);
    }
}
