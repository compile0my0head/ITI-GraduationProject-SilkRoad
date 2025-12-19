using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.BillingAddress)
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.PSID)
            .HasMaxLength(100);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Soft delete
        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(c => c.Store)
            .WithMany(s => s.Customers)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(c => c.StoreId);
        builder.HasIndex(c => c.PSID);
        builder.HasIndex(c => c.IsDeleted);
    }
}
