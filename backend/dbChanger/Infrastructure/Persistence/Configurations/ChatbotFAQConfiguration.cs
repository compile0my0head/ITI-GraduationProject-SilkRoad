using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ChatbotFAQConfiguration : IEntityTypeConfiguration<ChatbotFAQ>
{
    public void Configure(EntityTypeBuilder<ChatbotFAQ> builder)
    {
        builder.ToTable("ChatbotFAQs");
        
        builder.HasKey(faq => faq.Id);
        
        builder.Property(faq => faq.Id)
            .HasColumnName("FAQID");
        
        builder.Property(faq => faq.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(faq => faq.Question)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(faq => faq.Answer)
            .IsRequired()
            .HasMaxLength(2000);
        
        builder.Property(faq => faq.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(faq => faq.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(faq => faq.DeletedAt);
        
        builder.Property(faq => faq.DeletedByUserId);
        
        // Relationships
        builder.HasOne(faq => faq.Store)
            .WithMany(s => s.ChatbotFAQs)
            .HasForeignKey(faq => faq.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

