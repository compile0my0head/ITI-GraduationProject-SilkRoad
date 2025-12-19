using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        
        // Composite primary key
        builder.HasKey(tm => new { tm.TeamId, tm.UserId });
        
        builder.Property(tm => tm.TeamId)
            .HasColumnName("TeamID");
        
        builder.Property(tm => tm.UserId)
            .HasColumnName("UserID");
        
        builder.Property(tm => tm.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(tm => tm.AddedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(tm => tm.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(tm => tm.DeletedAt);
        
        builder.Property(tm => tm.DeletedByUserId);
        
        // Relationships
        builder.HasOne(tm => tm.Team)
            .WithMany(t => t.TeamMembers)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FIXED: Change User relationship from Cascade to NoAction
        builder.HasOne(tm => tm.User)
            .WithMany(u => u.TeamMemberships)
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade to NoAction
    }
}

