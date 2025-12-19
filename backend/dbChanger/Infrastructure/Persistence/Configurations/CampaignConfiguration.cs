using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("CampaignID");
        
        builder.Property(c => c.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(c => c.CampaignName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(c => c.CampaignBannerUrl)
            .HasMaxLength(1000);
        
        builder.Property(c => c.AssignedProductID)
            .HasColumnName("AssignedProductID");
        
        builder.Property(c => c.CampaignStage)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CampaignStage.Draft);
        
        builder.Property(c => c.Goal)
            .HasMaxLength(100);
        
        builder.Property(c => c.TargetAudience)
            .HasColumnType("nvarchar(max)");
        
        builder.Property(c => c.CreatedByUserID)
            .HasColumnName("CreatedByUserID");
        
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(c => c.DeletedAt);
        
        builder.Property(c => c.DeletedByUserId);
        
        // Relationships
        builder.HasOne(c => c.Store)
            .WithMany(s => s.Campaigns)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(c => c.AssignedProduct)
            .WithMany()
            .HasForeignKey(c => c.AssignedProductID)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(c => c.CreatedBy)
            .WithMany(u => u.CreatedCampaigns)
            .HasForeignKey(c => c.CreatedByUserID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

