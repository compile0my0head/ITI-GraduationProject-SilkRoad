using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnName("TeamID");
        
        builder.Property(t => t.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(t => t.TeamName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(t => t.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(t => t.DeletedAt);
        
        builder.Property(t => t.DeletedByUserId);
        
        // Relationships
        builder.HasOne(t => t.Store)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

