using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class SocialPlatformConfiguration : IEntityTypeConfiguration<SocialPlatform>
{
    public void Configure(EntityTypeBuilder<SocialPlatform> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.PlatformName)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(sp => sp.ExternalPageID)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.PageName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.AccessToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(sp => sp.IsConnected)
            .IsRequired();

        builder.Property(sp => sp.UpdatedAt)
            .IsRequired();

        builder.Property(sp => sp.CreatedAt)
            .IsRequired();

        // Soft delete
        builder.Property(sp => sp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(sp => sp.Store)
            .WithMany(s => s.Platforms)
            .HasForeignKey(sp => sp.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sp => sp.StoreId);
        builder.HasIndex(sp => sp.PlatformName);
        builder.HasIndex(sp => sp.ExternalPageID);
        builder.HasIndex(sp => sp.IsConnected);
        builder.HasIndex(sp => sp.IsDeleted);
    }
}
