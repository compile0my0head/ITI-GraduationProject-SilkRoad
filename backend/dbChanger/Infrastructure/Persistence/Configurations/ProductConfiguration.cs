using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("ProductID");
        
        builder.Property(p => p.ExternalProductID)
            .HasMaxLength(50);
        
        builder.Property(p => p.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasColumnName("ProductName")
            .HasMaxLength(500);
        
        builder.Property(p => p.Description)
            .HasColumnName("ProductDescription")
            .HasMaxLength(2000);
        
        // Simple decimal mapping for Price
        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnName("ProductPrice")
            .HasColumnType("decimal(18,2)");
        
        builder.Property(p => p.InStock)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(p => p.Brand)
            .HasMaxLength(50);
        
        builder.Property(p => p.ImageUrl)
            .HasMaxLength(1000);
        
        builder.Property(p => p.Condition)
            .HasMaxLength(50);
        
        builder.Property(p => p.Url)
            .HasMaxLength(1000);
        
        builder.Property(p => p.RetailerId)
            .HasMaxLength(100);
        
        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(p => p.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(p => p.DeletedAt);
        
        builder.Property(p => p.DeletedByUserId);
        
        // Relationships
        builder.HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

