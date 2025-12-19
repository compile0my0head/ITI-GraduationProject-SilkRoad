using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Products;

/// <summary>
/// Data Transfer Object for Product
/// This is what gets returned to the API consumer (frontend)
/// DTOs help separate domain models from API contracts
/// </summary>
public record ProductDto
{
    public Guid Id { get; init; }
    public string? ExternalProductID { get; init; }
    public Guid StoreId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
    public bool InStock { get; init; }
    public string? Brand { get; init; }
    public string? ImageUrl { get; init; }
    public string? Condition { get; init; }
    public string? Url { get; init; }
    public string? RetailerId { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a new product
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateProductRequest
{
    [MaxLength(50)]
    public string? ExternalProductID { get; init; }
    
    [Required]
    [MaxLength(200)]
    [MinLength(3)]
    public string ProductName { get; init; } = string.Empty;
    
    [MaxLength(5000)]
    public string ProductDescription { get; init; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal ProductPrice { get; init; }
    
    public bool InStock { get; init; } = true;
    
    [MaxLength(50)]
    public string? Brand { get; init; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; init; }
    
    [MaxLength(50)]
    public string? Condition { get; init; }
    
    [MaxLength(500)]
    public string? Url { get; init; }
    
    [MaxLength(50)]
    public string? RetailerId { get; init; }
}

public record UpdateProductRequest
{
    [MaxLength(50)]
    public string? ExternalProductID { get; init; }
    
    [MaxLength(200)]
    [MinLength(3)]
    public string? ProductName { get; init; }
    
    [MaxLength(5000)]
    public string? ProductDescription { get; init; }
    
    [Range(0, double.MaxValue)]
    public decimal? ProductPrice { get; init; }
    
    public bool? InStock { get; init; }
    
    [MaxLength(50)]
    public string? Brand { get; init; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; init; }
    
    [MaxLength(50)]
    public string? Condition { get; init; }
    
    [MaxLength(500)]
    public string? Url { get; init; }
    
    [MaxLength(50)]
    public string? RetailerId { get; init; }
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
