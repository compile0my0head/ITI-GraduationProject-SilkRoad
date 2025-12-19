using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Teams;

public record TeamDto
{
    public Guid Id { get; init; }
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = string.Empty;
    public string TeamName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int MemberCount { get; init; }
}

/// <summary>
/// Request DTO for creating a new team
/// StoreId is automatically injected from X-Store-ID header via IStoreContext
/// </summary>
public record CreateTeamRequest
{
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string TeamName { get; init; } = string.Empty;
}

public record UpdateTeamRequest
{
    [MaxLength(100)]
    [MinLength(3)]
    public string? TeamName { get; init; }
}

public record TeamMemberDto
{
    public Guid TeamId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
}

public record AddTeamMemberRequest
{
    [Required]
    public Guid UserId { get; init; }
    
    [Required]
    public TeamRole Role { get; init; }
}

public record UpdateTeamMemberRoleRequest
{
    [Required]
    public TeamRole Role { get; init; }
}
