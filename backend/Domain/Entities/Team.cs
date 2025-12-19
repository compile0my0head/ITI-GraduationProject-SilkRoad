using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Team : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string TeamName { get; set; } = string.Empty;
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new HashSet<TeamMember>();
}

