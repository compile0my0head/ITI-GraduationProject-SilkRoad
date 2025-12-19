using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Junction table for Many-to-Many relationship between Team and User
/// </summary>
public class TeamMember
{
    public int TeamId { get; set; }
    public int UserId { get; set; }
    public TeamRole Role { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Soft delete properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
    
    // Navigation properties
    public Team? Team { get; set; }
    public User? User { get; set; }
}

