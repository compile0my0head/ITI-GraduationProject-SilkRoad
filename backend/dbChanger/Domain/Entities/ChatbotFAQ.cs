using Domain.Common;

namespace Domain.Entities;

public class ChatbotFAQ : BaseEntity
{
    public int StoreId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    
    // Navigation property
    public Store? Store { get; set; }
}

