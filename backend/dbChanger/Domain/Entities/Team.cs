using Domain.Common;

namespace Domain.Entities;

public class Team : BaseEntity
{
    public int StoreId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    
    // Navigation properties
    public Store? Store { get; set; }
    public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}

