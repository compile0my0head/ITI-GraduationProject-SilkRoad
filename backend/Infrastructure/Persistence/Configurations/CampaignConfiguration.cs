using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CampaignName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CampaignBannerUrl)
            .HasMaxLength(500);

        builder.Property(c => c.CampaignStage)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Goal)
            .HasMaxLength(100);

        builder.Property(c => c.TargetAudience)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();

        builder.Property(c => c.ScheduledStartAt);

        builder.Property(c => c.ScheduledEndAt);

        builder.Property(c => c.IsSchedulingEnabled)
            .IsRequired()
            .HasDefaultValue(false);

        // Soft delete
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(c => c.Store)
            .WithMany(s => s.Campaigns)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AssignedProduct)
            .WithMany(p => p.Campaigns)
            .HasForeignKey(c => c.AssignedProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.CreatedBy)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(c => c.Posts)
            .WithOne(cp => cp.Campaign)
            .HasForeignKey(cp => cp.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.StoreId);
        builder.HasIndex(c => c.CampaignStage);
        builder.HasIndex(c => c.CreatedByUserId);
        builder.HasIndex(c => c.IsDeleted);
    }
}
