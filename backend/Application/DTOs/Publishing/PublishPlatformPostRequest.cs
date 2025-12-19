using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Publishing;

/// <summary>
/// Request DTO for publishing a post to a social media platform
/// Used to pass data from Application layer to Infrastructure publishers
/// </summary>
public record PublishPlatformPostRequest
{
    /// <summary>
    /// Post caption/message content
    /// </summary>
    [Required]
    public string Caption { get; init; } = string.Empty;

    /// <summary>
    /// Optional image URL to include with the post
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Platform access token for authentication
    /// </summary>
    [Required]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// External platform page/account identifier
    /// </summary>
    [Required]
    public string ExternalPageId { get; init; } = string.Empty;

    /// <summary>
    /// CampaignPostPlatform ID for logging/tracking purposes
    /// </summary>
    public Guid CampaignPostPlatformId { get; init; }
}
