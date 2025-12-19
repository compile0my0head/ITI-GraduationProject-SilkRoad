using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;


public class RefreshToken : BaseEntity
{

    [Required]
    public Guid UserId { get; set; }


    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string JwtId { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}
