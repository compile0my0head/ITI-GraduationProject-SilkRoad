using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CampaignPostConfiguration : IEntityTypeConfiguration<CampaignPost>
{
    public void Configure(EntityTypeBuilder<CampaignPost> builder)
    {
        builder.ToTable("CampaignPosts");
        
        builder.HasKey(cp => cp.Id);
        
        builder.Property(cp => cp.Id)
            .HasColumnName("CampaignPostID");
        
        builder.Property(cp => cp.CampaignID)
            .HasColumnName("CampaignID");
        
        builder.Property(cp => cp.PostCaption)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
        
        builder.Property(cp => cp.PostImageUrl)
            .HasMaxLength(1000);
        
        builder.Property(cp => cp.ScheduledAt);
        
        builder.Property(cp => cp.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(cp => cp.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(cp => cp.DeletedAt);
        
        builder.Property(cp => cp.DeletedByUserId);
        
        // Relationships
        builder.HasOne(cp => cp.Campaign)
            .WithMany(c => c.CampaignPosts)
            .HasForeignKey(cp => cp.CampaignID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

