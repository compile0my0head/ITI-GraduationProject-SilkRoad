using Domain.Common;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;


public class User : IdentityUser<Guid>
{

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Store> OwnedStores { get; set; } = new List<Store>();

    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

