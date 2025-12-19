using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AutomationTaskConfiguration : IEntityTypeConfiguration<AutomationTask>
{
    public void Configure(EntityTypeBuilder<AutomationTask> builder)
    {
        builder.HasKey(at => at.Id);

        builder.Property(at => at.TaskType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(at => at.CronExpression)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(at => at.IsActive)
            .IsRequired();

        builder.Property(at => at.CreatedAt)
            .IsRequired();

        // Soft delete
        builder.Property(at => at.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(at => at.Store)
            .WithMany(s => s.AutomationTasks)
            .HasForeignKey(at => at.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(at => at.RelatedCampaignPost)
            .WithMany(cp => cp.AutomationTasks)
            .HasForeignKey(at => at.RelatedCampaignPostId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(at => at.StoreId);
        builder.HasIndex(at => at.TaskType);
        builder.HasIndex(at => at.IsActive);
        builder.HasIndex(at => at.IsDeleted);
    }
}
