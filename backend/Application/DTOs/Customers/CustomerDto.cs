using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Customers;

public record CustomerDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; init; } = string.Empty;
    
    [MaxLength(500)]
    public string? BillingAddress { get; init; }
    
    [MaxLength(20)]
    public string? Phone { get; init; }
    
    [MaxLength(100)]
    public string? PSID { get; init; }
    
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a new customer
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateCustomerRequest
{
    [Required]
    [MaxLength(100)]
    [MinLength(2)]
    public string CustomerName { get; init; } = string.Empty;
    
    [MaxLength(500)]
    public string? BillingAddress { get; init; }
    
    [MaxLength(20)]
    [Phone]
    public string? Phone { get; init; }
    
    [MaxLength(100)]
    public string? PSID { get; init; }
}

public record UpdateCustomerRequest
{
    [MaxLength(100)]
    [MinLength(2)]
    public string? CustomerName { get; init; }
    
    [MaxLength(500)]
    public string? BillingAddress { get; init; }
    
    [MaxLength(20)]
    [Phone]
    public string? Phone { get; init; }
    
    [MaxLength(100)]
    public string? PSID { get; init; }
}
