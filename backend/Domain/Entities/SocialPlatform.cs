using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class SocialPlatform : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public PlatformName PlatformName { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ExternalPageID { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string PageName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string AccessToken { get; set; } = string.Empty;
    
    public bool IsConnected { get; set; } = true;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Store Store { get; set; } = null!;
}

