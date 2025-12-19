using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CampaignPostConfiguration : IEntityTypeConfiguration<CampaignPost>
{
    public void Configure(EntityTypeBuilder<CampaignPost> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.PostCaption)
            .IsRequired();

        builder.Property(cp => cp.PostImageUrl)
            .HasMaxLength(500);

        builder.Property(cp => cp.CreatedAt)
            .IsRequired();

        builder.Property(cp => cp.PublishStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue(PublishStatus.Pending.ToString());

        builder.Property(cp => cp.LastPublishError)
            .HasColumnType("nvarchar(max)");

        // Soft delete
        builder.Property(cp => cp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(cp => cp.Campaign)
            .WithMany(c => c.Posts)
            .HasForeignKey(cp => cp.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cp => cp.AutomationTasks)
            .WithOne(at => at.RelatedCampaignPost)
            .HasForeignKey(at => at.RelatedCampaignPostId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(cp => cp.PlatformPosts)
            .WithOne(cpp => cpp.CampaignPost)
            .HasForeignKey(cpp => cpp.CampaignPostId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cp => cp.CampaignId);
        builder.HasIndex(cp => cp.ScheduledAt);
        builder.HasIndex(cp => cp.IsDeleted);
    }
}
