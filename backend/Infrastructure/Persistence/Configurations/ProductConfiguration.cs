using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ExternalProductID)
            .HasMaxLength(50);

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.ProductDescription)
            .HasMaxLength(5000);

        builder.Property(p => p.ProductPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.InStock)
            .IsRequired();

        builder.Property(p => p.Brand)
            .HasMaxLength(50);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Condition)
            .HasMaxLength(50);

        builder.Property(p => p.Url)
            .HasMaxLength(500);

        builder.Property(p => p.RetailerId)
            .HasMaxLength(50);

        // Soft delete
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Campaigns)
            .WithOne(c => c.AssignedProduct)
            .HasForeignKey(c => c.AssignedProductId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(p => p.StoreId);
        builder.HasIndex(p => p.ExternalProductID);
        builder.HasIndex(p => p.IsDeleted);
    }
}
