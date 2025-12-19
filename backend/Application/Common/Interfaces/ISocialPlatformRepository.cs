using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISocialPlatformRepository
{
    Task<List<SocialPlatform>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SocialPlatform?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SocialPlatform>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all connected (IsConnected = true) social platforms for a specific store
    /// Used when creating CampaignPost to auto-generate CampaignPostPlatform records
    /// </summary>
    Task<List<SocialPlatform>> GetConnectedPlatformsByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets social platform by external page ID (Facebook Page ID)
    /// Used for chatbot order integration to determine store
    /// </summary>
    Task<SocialPlatform?> GetByExternalPageIdAsync(string externalPageId, CancellationToken cancellationToken = default);
    
    Task<SocialPlatform> AddAsync(SocialPlatform socialPlatform, CancellationToken cancellationToken = default);
    Task UpdateAsync(SocialPlatform socialPlatform, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
