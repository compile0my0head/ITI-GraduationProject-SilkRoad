using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasColumnName("CustomerID");
        
        builder.Property(c => c.StoreId)
            .HasColumnName("StoreID");
        
        builder.Property(c => c.CustomerName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(c => c.BillingAddress)
            .HasMaxLength(500);
        
        builder.Property(c => c.Phone)
            .HasMaxLength(20);
        
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(c => c.DeletedAt);
        
        builder.Property(c => c.DeletedByUserId);
        
        // Relationships
        builder.HasOne(c => c.Store)
            .WithMany(s => s.Customers)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

