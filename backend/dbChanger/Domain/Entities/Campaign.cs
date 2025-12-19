using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Campaign : BaseEntity
{
    public int StoreId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string? CampaignBannerUrl { get; set; }
    public int? AssignedProductID { get; set; }
    public CampaignStage CampaignStage { get; set; } = CampaignStage.Draft;
    public string? Goal { get; set; }
    public string? TargetAudience { get; set; } // JSON string
    public int? CreatedByUserID { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Store? Store { get; set; }
    public Product? AssignedProduct { get; set; }
    public User? CreatedBy { get; set; }
    public ICollection<CampaignPost> CampaignPosts { get; set; } = new List<CampaignPost>();
}
