using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for Product
/// Defined in APPLICATION layer but implemented in INFRASTRUCTURE layer
/// This follows Dependency Inversion Principle (SOLID)
/// </summary>
public interface IProductRepository
{
    // Get all products (no filtering for this simple example)
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    
    // Get products by store ID
    Task<List<Product>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    
    // Get single product by ID
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Search product by name (case-insensitive contains match)
    Task<Product?> GetByNameAsync(string productName, Guid storeId, CancellationToken cancellationToken = default);
    
    // Add new product
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    
    // Update existing product
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    
    // Delete product
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
