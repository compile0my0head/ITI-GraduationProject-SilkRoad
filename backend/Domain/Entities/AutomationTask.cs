using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class AutomationTask : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public TaskType TaskType { get; set; }
    
    public Guid? RelatedCampaignPostId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CronExpression { get; set; } = string.Empty;
    
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public CampaignPost? RelatedCampaignPost { get; set; }
}