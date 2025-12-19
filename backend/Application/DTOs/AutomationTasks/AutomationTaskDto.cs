using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AutomationTasks;

public record AutomationTaskDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    
    [Required]
    public string TaskType { get; init; } = string.Empty;
    
    public Guid? RelatedCampaignPostId { get; init; }
    
    [Required]
    [MaxLength(100)]
    public string CronExpression { get; init; } = string.Empty;
    
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating an automation task
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateAutomationTaskRequest
{
    [Required]
    public string TaskType { get; init; } = string.Empty;
    
    public Guid? RelatedCampaignPostId { get; init; }
    
    [Required]
    [MaxLength(100)]
    public string CronExpression { get; init; } = string.Empty;
    
    public bool IsActive { get; init; } = true;
}

public record UpdateAutomationTaskRequest
{
    [MaxLength(100)]
    public string? CronExpression { get; init; }
    
    public bool? IsActive { get; init; }
}
