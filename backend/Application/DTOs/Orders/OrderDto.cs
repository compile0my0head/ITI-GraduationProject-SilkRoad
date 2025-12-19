using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Orders;

public record OrderDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public OrderStatus Status { get; init; }
    public string StatusDisplayName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a new order
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// Status defaults to Pending
/// </summary>
public record CreateOrderRequest
{
    [Required]
    public Guid CustomerId { get; init; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalPrice { get; init; }
}

public record UpdateOrderRequest
{
    [Range(0, double.MaxValue)]
    public decimal? TotalPrice { get; init; }
}
