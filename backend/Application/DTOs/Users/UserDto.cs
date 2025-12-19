using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Users;

public record UserDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public int OwnedStoresCount { get; init; }
}

public record CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string FullName { get; init; } = string.Empty;
    
    [Phone]
    public string? PhoneNumber { get; init; }
}

public record UpdateUserRequest
{
    [MaxLength(100)]
    public string? FullName { get; init; }
    
    [Phone]
    public string? PhoneNumber { get; init; }
}
