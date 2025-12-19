using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.OrderProducts;

public record OrderProductDto
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public record AddProductToOrderRequest
{
    [Required]
    public Guid OrderId { get; init; }
    
    [Required]
    public Guid ProductId { get; init; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; init; }
}

public record UpdateOrderProductQuantityRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }
}
