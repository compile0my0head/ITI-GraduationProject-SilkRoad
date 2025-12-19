using Application.DTOs.SocialPlatforms;

namespace Application.Common.Interfaces;

public interface ISocialPlatformService
{
    // Basic CRUD Operations
    Task<List<SocialPlatformDto>> GetPlatformsByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    
    Task<SocialPlatformDto?> GetPlatformByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<SocialPlatformDto> CreatePlatformAsync(CreateSocialPlatformRequest request, CancellationToken cancellationToken = default);
    
    Task<SocialPlatformDto> UpdatePlatformAsync(Guid id, UpdateSocialPlatformRequest request, CancellationToken cancellationToken = default);
    
    Task<SocialPlatformDto> DisconnectPlatformAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task DeletePlatformAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SocialPlatformDto> ConnectFacebookAsync(ConnectFacebookRequest request, CancellationToken cancellationToken = default);

    Task<SocialPlatformDto> ConnectInstagramAsync(ConnectInstagramRequest request, CancellationToken cancellationToken = default);

}
