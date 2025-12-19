using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ChatbotFAQs;

public record ChatbotFAQDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    
    [MaxLength(100)]
    public string? PSID { get; init; }
    
    [Required]
    public string Question { get; init; } = string.Empty;
    
    [Required]
    public string Answer { get; init; } = string.Empty;
    
    [Required]
    public string MessageType { get; init; } = "Question";
    
    public DateTime CreatedAt { get; init; }
}

public record CreateChatbotFAQRequest
{
    [MaxLength(100)]
    public string? PSID { get; init; }
    
    [Required]
    [MinLength(5)]
    public string Question { get; init; } = string.Empty;
    
    [Required]
    [MinLength(5)]
    public string Answer { get; init; } = string.Empty;
    
    public string MessageType { get; init; } = "Question";
}

public record UpdateChatbotFAQRequest
{
    [MaxLength(100)]
    public string? PSID { get; init; }
    
    [MinLength(5)]
    public string? Question { get; init; }
    
    [MinLength(5)]
    public string? Answer { get; init; }
    
    public string? MessageType { get; init; }
}