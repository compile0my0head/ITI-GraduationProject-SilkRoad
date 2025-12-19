using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class ChatbotFAQ : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [MaxLength(100)]
    public string? PSID { get; set; }
    
    [Required]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    public string Answer { get; set; } = string.Empty;
    
    public MessageType MessageType { get; set; } = MessageType.Question;
    
    // Navigation property
    public Store Store { get; set; } = null!;
}

