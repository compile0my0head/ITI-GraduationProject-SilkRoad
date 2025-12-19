using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        // Apply to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // another way to check is  using "modelBuilder.Entity<T>() .HasQueryFilter(e => !e.IsDeleted),"
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter
                );
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
        
        // Manually apply to TeamMember (doesn't inherit BaseEntity)
        {
            var parameter = Expression.Parameter(typeof(TeamMember), "e");
            var property = Expression.Property(parameter, nameof(TeamMember.IsDeleted));
            var filter = Expression.Lambda<Func<TeamMember, bool>>(
                Expression.Equal(property, Expression.Constant(false)),
                parameter
            );
            modelBuilder.Entity<TeamMember>().HasQueryFilter(filter);
        }
        
        // Manually apply to OrderProduct (doesn't inherit BaseEntity)
        {
            var parameter = Expression.Parameter(typeof(OrderProduct), "e");
            var property = Expression.Property(parameter, nameof(OrderProduct.IsDeleted));
            var filter = Expression.Lambda<Func<OrderProduct, bool>>(
                Expression.Equal(property, Expression.Constant(false)),
                parameter
            );
            modelBuilder.Entity<OrderProduct>().HasQueryFilter(filter);
        }
    }
}