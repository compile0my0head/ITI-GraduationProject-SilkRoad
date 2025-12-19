using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Id)
            .HasColumnName("StoreID");
        
        builder.Property(s => s.StoreName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(s => s.OwnerUserId)
            .HasColumnName("OwnerUserID");
        
        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETDATE()");
        
        // Soft delete properties
        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false);
        
        builder.Property(s => s.DeletedAt);
        
        builder.Property(s => s.DeletedByUserId);
    }
}

