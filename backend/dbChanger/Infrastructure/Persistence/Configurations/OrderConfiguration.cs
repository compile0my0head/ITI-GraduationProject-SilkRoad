using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id)
            .HasColumnName("OrderID");
        
        builder.Property(o => o.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(o => o.CustomerID)
            .HasColumnName("CustomerID");
        
        builder.Property(o => o.TotalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        
        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(o => o.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(o => o.DeletedAt);
        
        builder.Property(o => o.DeletedByUserId);
        
        // Relationships
        builder.HasOne(o => o.Store)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerID)
            .OnDelete(DeleteBehavior.NoAction);
        
        // Check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_Order_TotalPrice", "[TotalPrice] >= 0"));
    }
}

