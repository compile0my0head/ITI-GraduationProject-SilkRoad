using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TeamName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Soft delete
        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(t => t.Store)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Members)
            .WithOne(tm => tm.Team)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.StoreId);
        builder.HasIndex(t => t.IsDeleted);
    }
}
