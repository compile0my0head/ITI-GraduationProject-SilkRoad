using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public record LoginRequestDto
{
    [EmailAddress]
    [Required]
    public string Email { get; init; }
    [Required]
    public string Password { get; init; }
}
