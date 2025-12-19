using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// CampaignPost - individual social media post within a campaign
/// Core entity for scheduled posting with Hangfire integration
/// </summary>
public class CampaignPost : BaseEntity
{
    /// <summary>
    /// Foreign key to parent campaign
    /// </summary>
    [Required]
    public Guid CampaignId { get; set; }

    /// <summary>
    /// Post text content / caption
    /// </summary>
    [Required]
    public string PostCaption { get; set; } = string.Empty;

    /// <summary>
    /// URL to image or video file
    /// </summary>
    [MaxLength(500)]
    public string? PostImageUrl { get; set; }

    /// <summary>
    /// When user wants this post published (UTC timezone)
    /// </summary>
    public DateTime? ScheduledAt { get; set; }
    
    /// <summary>
    /// Publishing status (Pending, Publishing, Published, Failed)
    /// Stored as string in DB but use PublishStatus enum in code
    /// </summary>
    [MaxLength(50)]
    public string PublishStatus { get; set; } = Domain.Enums.PublishStatus.Pending.ToString();
    
    /// <summary>
    /// When the post was actually published
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// Error message if publication failed
    /// </summary>
    public string? LastPublishError { get; set; }

    // Navigation properties
    public Campaign Campaign { get; set; } = null!;
    public ICollection<AutomationTask> AutomationTasks { get; set; } = new HashSet<AutomationTask>();
    public ICollection<CampaignPostPlatform> PlatformPosts { get; set; } = new HashSet<CampaignPostPlatform>();
}
