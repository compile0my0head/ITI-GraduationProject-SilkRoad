using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Stores;

public record StoreDto
{
    public Guid Id { get; init; }
    
    [Required]
    [MaxLength(200)]
    [MinLength(4)]
    public string StoreName { get; init; } = default!;
    
    public Guid OwnerUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CreateStoreRequest
{
    [Required]
    [MaxLength(200)]
    [MinLength(4)]
    public string StoreName { get; init; } = default!;
}

public record UpdateStoreRequest
{
    [MaxLength(200)]
    [MinLength(4)]
    public string? StoreName { get; init; }
}
