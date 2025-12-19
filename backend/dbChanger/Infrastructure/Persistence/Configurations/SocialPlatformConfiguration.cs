using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SocialPlatformConfiguration : IEntityTypeConfiguration<SocialPlatform>
{
    public void Configure(EntityTypeBuilder<SocialPlatform> builder)
    {
        builder.ToTable("SocialPlatforms");
        
        builder.HasKey(sp => sp.Id);
        
        builder.Property(sp => sp.Id)
            .HasColumnName("PlatformID");
        
        builder.Property(sp => sp.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(sp => sp.PlatformName)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(sp => sp.ExternalPageID)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(sp => sp.PageName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(sp => sp.AccessToken)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(sp => sp.IsConnected)
            .HasColumnType("BIT")
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(sp => sp.UpdatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        builder.Property(sp => sp.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(sp => sp.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(sp => sp.DeletedAt);
        
        builder.Property(sp => sp.DeletedByUserId);
        
        // Relationships
        builder.HasOne(sp => sp.Store)
            .WithMany(s => s.Platforms)
            .HasForeignKey(sp => sp.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

