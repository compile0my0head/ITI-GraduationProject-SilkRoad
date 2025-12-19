using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for CampaignPostPlatform junction table
/// No soft delete - cascade delete from CampaignPost only
/// </summary>
public class CampaignPostPlatformConfiguration : IEntityTypeConfiguration<CampaignPostPlatform>
{
    public void Configure(EntityTypeBuilder<CampaignPostPlatform> builder)
    {
        builder.HasKey(cpp => cpp.Id);

        builder.Property(cpp => cpp.CampaignPostId)
            .IsRequired();

        builder.Property(cpp => cpp.PlatformId)
            .IsRequired();

        builder.Property(cpp => cpp.ExternalPostId)
            .HasMaxLength(200);

        builder.Property(cpp => cpp.PublishStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue(PublishStatus.Pending.ToString());

        builder.Property(cpp => cpp.ScheduledAt)
            .IsRequired();

        builder.Property(cpp => cpp.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        // Relationships
        builder.HasOne(cpp => cpp.CampaignPost)
            .WithMany(cp => cp.PlatformPosts)
            .HasForeignKey(cpp => cpp.CampaignPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cpp => cpp.Platform)
            .WithMany()
            .HasForeignKey(cpp => cpp.PlatformId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(cpp => cpp.CampaignPostId);
        builder.HasIndex(cpp => cpp.PlatformId);
        builder.HasIndex(cpp => cpp.PublishStatus);
        builder.HasIndex(cpp => cpp.ScheduledAt);
        builder.HasIndex(cpp => new { cpp.CampaignPostId, cpp.PlatformId })
            .IsUnique();
    }
}
