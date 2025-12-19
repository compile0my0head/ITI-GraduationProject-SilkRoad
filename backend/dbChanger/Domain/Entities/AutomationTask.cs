using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class AutomationTask : BaseEntity
{
    public int StoreId { get; set; }
    public TaskType TaskType { get; set; }
    public int? RelatedCampaignPostID { get; set; }
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Navigation properties
    public Store? Store { get; set; }
    public CampaignPost? RelatedCampaignPost { get; set; }
}

