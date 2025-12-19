namespace Application.Services.Publishing;

/// <summary>
/// Service for publishing CampaignPostPlatform records to social media platforms
/// </summary>
public interface IPlatformPublishingService
{
    /// <summary>
    /// Processes all due CampaignPostPlatform records for publishing
    /// This is the core method called by background jobs
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessDuePlatformPostsAsync(CancellationToken cancellationToken = default);
}
