using Domain.Common;

namespace Domain.Entities;

public class CampaignPost : BaseEntity
{
    public int CampaignID { get; set; }
    public string PostCaption { get; set; } = string.Empty;
    public string? PostImageUrl { get; set; }
    public DateTime? ScheduledAt { get; set; }
    
    // Navigation properties
    public Campaign? Campaign { get; set; }
    public ICollection<AutomationTask> AutomationTasks { get; set; } = new List<AutomationTask>();
}

