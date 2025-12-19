using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Campaign : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string CampaignName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? CampaignBannerUrl { get; set; }
    
    public Guid? AssignedProductId { get; set; }
    
    public CampaignStage CampaignStage { get; set; } = CampaignStage.Draft;
    
    [MaxLength(100)]
    public string? Goal { get; set; }
    
    public string? TargetAudience { get; set; }
    
    public Guid? CreatedByUserId { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Scheduling columns
    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? ScheduledEndAt { get; set; }
    public bool IsSchedulingEnabled { get; set; } = false;
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public Product? AssignedProduct { get; set; }
    public User? CreatedBy { get; set; }
    public ICollection<CampaignPost> Posts { get; set; } = new HashSet<CampaignPost>();
}
