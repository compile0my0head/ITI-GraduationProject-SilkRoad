using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {
        builder.ToTable("OrderProducts");
        
        // Composite primary key
        builder.HasKey(op => new { op.OrderID, op.ProductID });
        
        builder.Property(op => op.OrderID)
            .HasColumnName("OrderID");
        
        builder.Property(op => op.ProductID)
            .HasColumnName("ProductID");
        
        builder.Property(op => op.Quantity)
            .IsRequired();
        
        // Soft delete properties
        builder.Property(op => op.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(op => op.DeletedAt);
        
        builder.Property(op => op.DeletedByUserId);
        
        // Relationships
        builder.HasOne(op => op.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(op => op.OrderID)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(op => op.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(op => op.ProductID)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_OrderProduct_Quantity", "[Quantity] > 0"));
    }
}

