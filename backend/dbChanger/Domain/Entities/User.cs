using Domain.Common;

namespace Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Store> OwnedStores { get; set; } = new List<Store>();
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Campaign> CreatedCampaigns { get; set; } = new List<Campaign>();
}

