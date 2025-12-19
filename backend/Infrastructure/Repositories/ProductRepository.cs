using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// Implementation of IProductRepository
/// This is in INFRASTRUCTURE layer - handles data access
/// Uses Entity Framework Core to interact with the database
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly SaasDbContext _context;

    public ProductRepository(SaasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.StoreId == storeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Store)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByNameAsync(string productName, Guid storeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return null;

        // Case-insensitive contains match
        return await _context.Products
            .FirstOrDefaultAsync(
                p => p.StoreId == storeId && 
                     EF.Functions.Like(p.ProductName, $"%{productName}%"), 
                cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        return product;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} not found.");
        }

        if (product.IsDeleted)
        {
            throw new InvalidOperationException($"Product with ID {id} is already deleted.");
        }

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
    }
}
