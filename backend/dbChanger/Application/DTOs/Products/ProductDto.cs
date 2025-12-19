namespace Application.DTOs.Products;

/// <summary>
/// Data Transfer Object for Product
/// This is what gets returned to the API consumer (frontend)
/// DTOs help separate domain models from API contracts
/// </summary>
public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; } // Flattened from Money value object
    public string Currency { get; init; } = "USD";
    public bool InStock { get; init; }
    public int StoreId { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Response wrapper for list of products
/// Provides additional metadata about the result
/// </summary>
public record GetProductsResponse
{
    public List<ProductDto> Products { get; init; } = new();
    public int TotalCount { get; init; }
    public string Message { get; init; } = "Products retrieved successfully";
}
