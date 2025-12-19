using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CampaignPosts;

public record CampaignPostDto
{
    public Guid Id { get; init; }
    
    public Guid CampaignId { get; init; }
    public string CampaignName { get; init; } = string.Empty;
    
    [Required]
    public string PostCaption { get; init; } = string.Empty;
    
    [MaxLength(500)]
    public string? PostImageUrl { get; init; }
    
    public DateTime? ScheduledAt { get; init; }
    
    public string PublishStatus { get; init; } = string.Empty;
    
    public DateTime? PublishedAt { get; init; }
    
    public string? LastPublishError { get; init; }
    
    public DateTime CreatedAt { get; init; }
}

public record CreateCampaignPostRequest
{
    [Required]
    public Guid CampaignId { get; init; }
    
    [Required]
    [MaxLength(5000)]
    [MinLength(1)]
    public string PostCaption { get; init; } = string.Empty;
    
    [MaxLength(500)]
    public string? PostImageUrl { get; init; }
    
    public DateTime? ScheduledAt { get; init; }
}

public record UpdateCampaignPostRequest
{
    public Guid? CampaignId { get; init; }

    [MaxLength(5000)]
    [MinLength(1)]
    public string? PostCaption { get; init; }
    
    [MaxLength(500)]
    public string? PostImageUrl { get; init; }
    
    public DateTime? ScheduledAt { get; init; }
}

