using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Campaigns;

public record CampaignDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    
    [Required]
    [MaxLength(200)]
    public string CampaignName { get; init; } = string.Empty;
    
    [MaxLength(500)]
    public string? CampaignBannerUrl { get; init; }
    
    public Guid? AssignedProductId { get; init; }
    public string? AssignedProductName { get; init; }
    
    [Required]
    public string CampaignStage { get; init; } = "Draft";
    
    [MaxLength(100)]
    public string? Goal { get; init; }

    public string? TargetAudience { get; init; }
    
    public Guid? CreatedByUserId { get; init; }
    public string? CreatedByUserName { get; init; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    public DateTime? ScheduledStartAt { get; init; }
    
    public DateTime? ScheduledEndAt { get; init; }
    
    public bool IsSchedulingEnabled { get; init; }
}

/// <summary>
/// Request DTO for creating a new campaign
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateCampaignRequest
{
    [Required]
    [MaxLength(200)]
    [MinLength(3)]
    public string CampaignName { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? CampaignBannerUrl { get; init; }

    public Guid? AssignedProductId { get; init; }

    public string? CampaignStage { get; init; }

    [MaxLength(100)]
    public string? Goal { get; init; }

    public string? TargetAudience { get; init; }

    public DateTime? ScheduledStartAt { get; init; }
    
    public DateTime? ScheduledEndAt { get; init; }
    
    public bool IsSchedulingEnabled { get; init; }
}

public record UpdateCampaignRequest
{
    [MaxLength(200)]
    [MinLength(3)]
    public string? CampaignName { get; init; }

    [MaxLength(500)]
    public string? CampaignBannerUrl { get; init; }

    public Guid? AssignedProductId { get; init; }

    public string? CampaignStage { get; init; }

    [MaxLength(100)]
    public string? Goal { get; init; }

    public string? TargetAudience { get; init; }

    public DateTime? ScheduledStartAt { get; init; }
    
    public DateTime? ScheduledEndAt { get; init; }
    
    public bool? IsSchedulingEnabled { get; init; }
}
