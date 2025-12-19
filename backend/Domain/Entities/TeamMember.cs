using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities;

public class TeamMember
{
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    public TeamRole Role { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    // Soft delete properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
    
    // Navigation properties
    public Team Team { get; set; } = null!;
    public User User { get; set; } = null!;
}
