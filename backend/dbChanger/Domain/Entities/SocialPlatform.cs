using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class SocialPlatform : BaseEntity
{
    public int StoreId { get; set; }
    public PlatformName PlatformName { get; set; }
    public string ExternalPageID { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public bool IsConnected { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Store? Store { get; set; }
}

