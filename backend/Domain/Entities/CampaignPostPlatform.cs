using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

/// <summary>
/// Junction table tracking per-platform publication status for each CampaignPost
/// No soft delete - hard delete only via cascade from CampaignPost
/// </summary>
public class CampaignPostPlatform
{
    /// <summary>
    /// Primary key
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Foreign key to CampaignPost
    /// </summary>
    [Required]
    public Guid CampaignPostId { get; set; }
    
    /// <summary>
    /// Foreign key to SocialPlatform
    /// </summary>
    [Required]
    public Guid PlatformId { get; set; }
    
    /// <summary>
    /// External post ID from the social platform (Facebook, Instagram, etc.)
    /// </summary>
    [MaxLength(200)]
    public string? ExternalPostId { get; set; }
    
    /// <summary>
    /// Publishing status (Pending, Publishing, Published, Failed)
    /// Stored as string in DB but use PublishStatus enum in code
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PublishStatus { get; set; } = Domain.Enums.PublishStatus.Pending.ToString();
    
    /// <summary>
    /// When this post is scheduled to be published to this platform
    /// </summary>
    [Required]
    public DateTime ScheduledAt { get; set; }
    
    /// <summary>
    /// When this post was actually published to this platform
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// Error message if publication to this platform failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public CampaignPost CampaignPost { get; set; } = null!;
    public SocialPlatform Platform { get; set; } = null!;
}
