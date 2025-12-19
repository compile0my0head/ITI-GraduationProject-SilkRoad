using Domain.Entities;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for CampaignPostPlatform entity
/// </summary>
public interface ICampaignPostPlatformRepository
{
    /// <summary>
    /// Gets all CampaignPostPlatform records that are due for publishing
    /// </summary>
    /// <param name="currentTime">Current UTC time to compare against</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of due platform posts with all required navigation properties loaded</returns>
    Task<List<CampaignPostPlatform>> GetDuePlatformPostsAsync(DateTime currentTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all CampaignPostPlatform records with navigation properties
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all platform posts</returns>
    Task<List<CampaignPostPlatform>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all CampaignPostPlatform records for a specific CampaignPost
    /// </summary>
    /// <param name="campaignPostId">CampaignPost ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of platform posts for the campaign post</returns>
    Task<List<CampaignPostPlatform>> GetByCampaignPostIdAsync(Guid campaignPostId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a CampaignPostPlatform by ID with navigation properties
    /// </summary>
    /// <param name="id">Platform post ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Platform post or null if not found</returns>
    Task<CampaignPostPlatform?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a CampaignPostPlatform record
    /// </summary>
    /// <param name="platformPost">Platform post to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(CampaignPostPlatform platformPost, CancellationToken cancellationToken = default);
}
