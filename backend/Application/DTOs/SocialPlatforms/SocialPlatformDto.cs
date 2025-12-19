using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.SocialPlatforms;

public record SocialPlatformDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    
    [Required]
    public string PlatformName { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ExternalPageID { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string PageName { get; init; } = string.Empty;
    
    public bool IsConnected { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Request DTO for creating a social platform connection
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateSocialPlatformRequest
{
    [Required]
    public string PlatformName { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ExternalPageID { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string PageName { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string AccessToken { get; init; } = string.Empty;
}

public record UpdateSocialPlatformRequest
{
    [MaxLength(200)]
    public string? PageName { get; init; }
    
    [MaxLength(2000)]
    public string? AccessToken { get; init; }
    
    public bool? IsConnected { get; init; }
}

/// <summary>
/// Request DTO for connecting Facebook account
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record ConnectFacebookRequest
{
    [Required]
    public string Code { get; init; } = string.Empty;
    
    public string? RedirectUri { get; init; }
}

/// <summary>
/// Request DTO for connecting Instagram account
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record ConnectInstagramRequest
{
    [Required]
    public string Code { get; init; } = string.Empty;
    
    public string? RedirectUri { get; init; }
}
