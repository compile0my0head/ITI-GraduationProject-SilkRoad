using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Orders;

/// <summary>
/// Request DTO for chatbot orders from n8n
/// PUBLIC endpoint - no authentication required
/// </summary>
public record ChatbotOrderRequest
{
    [Required]
    public ChatbotCustomerInfo Customer { get; set; } = null!;
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<ChatbotOrderItem> Items { get; set; } = new();
    
    [Required]
    public string PageId { get; set; } = string.Empty;
}

public record ChatbotCustomerInfo
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
    
    [Required]
    public string Psid { get; set; } = string.Empty;
}

public record ChatbotOrderItem
{
    [Required]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
