using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StoreName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.OwnerUserId)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // Soft delete fields
        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(s => s.Owner)
            .WithMany(u => u.OwnedStores)
            .HasForeignKey(s => s.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Store)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Teams)
            .WithOne(t => t.Store)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Campaigns)
            .WithOne(c => c.Store)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Customers)
            .WithOne(c => c.Store)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Orders)
            .WithOne(o => o.Store)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Platforms)
            .WithOne(sp => sp.Store)
            .HasForeignKey(sp => sp.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.FAQs)
            .WithOne(f => f.Store)
            .HasForeignKey(f => f.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.AutomationTasks)
            .WithOne(at => at.Store)
            .HasForeignKey(at => at.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.OwnerUserId);
        builder.HasIndex(s => s.IsDeleted);
    }
}
