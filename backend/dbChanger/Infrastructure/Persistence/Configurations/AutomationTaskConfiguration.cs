using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AutomationTaskConfiguration : IEntityTypeConfiguration<AutomationTask>
{
    public void Configure(EntityTypeBuilder<AutomationTask> builder)
    {
        builder.ToTable("AutomationTasks");
        
        builder.HasKey(at => at.Id);
        
        builder.Property(at => at.Id)
            .HasColumnName("TaskID");
        
        builder.Property(at => at.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(at => at.TaskType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(at => at.RelatedCampaignPostID)
            .HasColumnName("RelatedCampaignPostID");
        
        builder.Property(at => at.CronExpression)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(at => at.IsActive)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(at => at.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(at => at.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(at => at.DeletedAt);
        
        builder.Property(at => at.DeletedByUserId);
        
        // Relationships
        builder.HasOne(at => at.Store)
            .WithMany(s => s.AutomationTasks)
            .HasForeignKey(at => at.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(at => at.RelatedCampaignPost)
            .WithMany(cp => cp.AutomationTasks)
            .HasForeignKey(at => at.RelatedCampaignPostID)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

