using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ChatbotFAQConfiguration : IEntityTypeConfiguration<ChatbotFAQ>
{
    public void Configure(EntityTypeBuilder<ChatbotFAQ> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.PSID)
            .HasMaxLength(100);

        builder.Property(f => f.Question)
            .IsRequired();

        builder.Property(f => f.Answer)
            .IsRequired();

        builder.Property(f => f.MessageType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Soft delete
        builder.Property(f => f.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(f => f.Store)
            .WithMany(s => s.FAQs)
            .HasForeignKey(f => f.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(f => f.StoreId);
        builder.HasIndex(f => f.PSID);
        builder.HasIndex(f => f.MessageType);
        builder.HasIndex(f => f.IsDeleted);
    }
}
