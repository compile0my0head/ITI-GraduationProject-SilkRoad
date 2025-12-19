using Application.Common.Interfaces;
using Application.DTOs.Publishing;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services.Publishing;

/// <summary>
/// Service responsible for publishing CampaignPostPlatform records to social media platforms.
/// Follows Clean Architecture - no HttpClient, no Hangfire, no platform-specific logic.
/// 
/// FACEBOOK API NOTES FOR DEMO:
/// - Facebook Page ID: Obtained when user connects their Facebook Page via OAuth (stored in SocialPlatform.ExternalPageID)
/// - Page Access Token: Long-lived token obtained during OAuth flow (stored in SocialPlatform.AccessToken)
/// - Graph API does NOT handle scheduling - this service schedules posts by publishing them at the right time
/// - Posts are published immediately to Facebook when this service runs
/// </summary>
public class PlatformPublishingService : IPlatformPublishingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnumerable<ISocialPlatformPublisher> _publishers;
    private readonly ILogger<PlatformPublishingService> _logger;

    public PlatformPublishingService(
        IUnitOfWork unitOfWork,
        IEnumerable<ISocialPlatformPublisher> publishers,
        ILogger<PlatformPublishingService> logger)
    {
        _unitOfWork = unitOfWork;
        _publishers = publishers;
        _logger = logger;
    }

    public async Task ProcessDuePlatformPostsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        try
        {
            // Fetch all due CampaignPostPlatform records
            // Repository GUARANTEES: CampaignPost, Campaign, and Platform navigation properties are loaded
            var duePlatformPosts = await _unitOfWork.CampaignPostPlatforms.GetDuePlatformPostsAsync(now, cancellationToken);

            _logger.LogInformation("Found {Count} due platform posts to process", duePlatformPosts.Count);

            // Group by CampaignPostId to update parent status after processing all platforms
            var postGroups = duePlatformPosts.GroupBy(pp => pp.CampaignPostId);

            foreach (var group in postGroups)
            {
                // Process each platform post for this campaign post
                foreach (var platformPost in group)
                {
                    await ProcessSinglePlatformPostAsync(platformPost, now, cancellationToken);
                }

                // After processing all platforms for this post, update the parent CampaignPost status
                await UpdateCampaignPostStatusAsync(group.Key, cancellationToken);
            }

            _logger.LogInformation("Completed processing {Count} due platform posts", duePlatformPosts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in platform publishing service");
            throw;
        }
    }

    private async Task ProcessSinglePlatformPostAsync(
        Domain.Entities.CampaignPostPlatform platformPost, 
        DateTime now, 
        CancellationToken cancellationToken)
    {
        try
        {
            // ⚠️ CRITICAL: Double-check status hasn't changed (race condition protection)
            // Reload from database to ensure we have the latest status
            var currentPlatformPost = await _unitOfWork.CampaignPostPlatforms.GetByIdAsync(platformPost.Id, cancellationToken);
            if (currentPlatformPost == null)
            {
                _logger.LogWarning("Platform post {PlatformPostId} not found", platformPost.Id);
                return;
            }

            // Skip if already being processed or already processed
            if (currentPlatformPost.PublishStatus != PublishStatus.Pending.ToString())
            {
                _logger.LogInformation(
                    "Skipping platform post {PlatformPostId} - status is {Status} (not Pending)", 
                    platformPost.Id, currentPlatformPost.PublishStatus);
                return;
            }

            // Update reference to use the fresh data
            platformPost = currentPlatformPost;

            // Trust repository contract - navigation properties are guaranteed to be loaded
            var campaign = platformPost.CampaignPost.Campaign;

            // Guard: Campaign scheduling must be enabled
            if (!campaign.IsSchedulingEnabled)
            {
                _logger.LogInformation(
                    "Skipping platform post {PlatformPostId} - scheduling disabled for campaign {CampaignId}", 
                    platformPost.Id, campaign.Id);
                return;
            }

            // Guard: Campaign must be within scheduled time window
            if (campaign.ScheduledStartAt.HasValue && now < campaign.ScheduledStartAt.Value)
            {
                _logger.LogInformation(
                    "Skipping platform post {PlatformPostId} - campaign {CampaignId} not started yet", 
                    platformPost.Id, campaign.Id);
                return;
            }

            if (campaign.ScheduledEndAt.HasValue && now > campaign.ScheduledEndAt.Value)
            {
                _logger.LogInformation(
                    "Skipping platform post {PlatformPostId} - campaign {CampaignId} has ended", 
                    platformPost.Id, campaign.Id);
                return;
            }

            // ⚠️ CRITICAL: Mark as Publishing IMMEDIATELY and save to prevent duplicate processing
            // This must happen BEFORE the actual publishing to prevent race conditions
            platformPost.PublishStatus = PublishStatus.Publishing.ToString();
            await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation(
                "Marked platform post {PlatformPostId} as Publishing to prevent duplicate processing", 
                platformPost.Id);

            // Select appropriate publisher based on platform name
            var platformName = platformPost.Platform.PlatformName.ToString();
            var publisher = _publishers.FirstOrDefault(p => 
                p.PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase));

            // Guard: Publisher must exist for platform
            if (publisher == null)
            {
                var error = $"No publisher found for platform: {platformName}";
                _logger.LogWarning("{Error} for platform post {PlatformPostId}", error, platformPost.Id);
                await MarkAsFailed(platformPost, error, cancellationToken);
                return;
            }

            // Build publishing request DTO
            var publishRequest = new PublishPlatformPostRequest
            {
                Caption = platformPost.CampaignPost.PostCaption,
                ImageUrl = platformPost.CampaignPost.PostImageUrl,
                AccessToken = platformPost.Platform.AccessToken,
                ExternalPageId = platformPost.Platform.ExternalPageID,
                CampaignPostPlatformId = platformPost.Id
            };

            // Publish to platform
            var result = await publisher.PublishAsync(publishRequest, cancellationToken);

            // Handle result
            if (result.IsSuccess)
            {
                platformPost.ExternalPostId = result.ExternalPostId;
                platformPost.PublishStatus = PublishStatus.Published.ToString();
                platformPost.PublishedAt = now;
                platformPost.ErrorMessage = null;
                
                await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully published platform post {PlatformPostId} to {Platform} with external ID {ExternalId}",
                    platformPost.Id, platformPost.Platform.PlatformName, result.ExternalPostId);
            }
            else
            {
                await MarkAsFailed(platformPost, result.ErrorMessage ?? "Unknown error", cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish platform post {PlatformPostId} to platform {Platform}",
                platformPost.Id, platformPost.Platform?.PlatformName);
            
            await MarkAsFailed(platformPost, ex.Message, cancellationToken);
        }
    }

    private async Task MarkAsFailed(
        Domain.Entities.CampaignPostPlatform platformPost, 
        string errorMessage, 
        CancellationToken cancellationToken)
    {
        platformPost.PublishStatus = PublishStatus.Failed.ToString();
        platformPost.ErrorMessage = errorMessage;
        await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the CampaignPost status based on the status of all its platform posts
    /// </summary>
    private async Task UpdateCampaignPostStatusAsync(Guid campaignPostId, CancellationToken cancellationToken)
    {
        var campaignPost = await _unitOfWork.CampaignPosts.GetByIdAsync(campaignPostId, cancellationToken);
        if (campaignPost == null)
        {
            _logger.LogWarning("CampaignPost {CampaignPostId} not found for status update", campaignPostId);
            return;
        }

        // Get all platform posts for this campaign post using optimized query
        var platformPosts = await _unitOfWork.CampaignPostPlatforms.GetByCampaignPostIdAsync(campaignPostId, cancellationToken);

        if (!platformPosts.Any())
        {
            _logger.LogWarning("No platform posts found for CampaignPost {CampaignPostId}", campaignPostId);
            return;
        }

        // Determine overall status based on platform post statuses
        var allPublished = platformPosts.All(pp => pp.PublishStatus == PublishStatus.Published.ToString());
        var anyFailed = platformPosts.Any(pp => pp.PublishStatus == PublishStatus.Failed.ToString());
        var anyPublishing = platformPosts.Any(pp => pp.PublishStatus == PublishStatus.Publishing.ToString());

        string newStatus;
        DateTime? publishedAt = null;
        string? errorMessage = null;

        if (allPublished)
        {
            newStatus = PublishStatus.Published.ToString();
            publishedAt = platformPosts.Max(pp => pp.PublishedAt); // Use the latest publish time
            _logger.LogInformation("All platforms published for CampaignPost {CampaignPostId}", campaignPostId);
        }
        else if (anyFailed && !anyPublishing)
        {
            newStatus = PublishStatus.Failed.ToString();
            var failedPosts = platformPosts.Where(pp => pp.PublishStatus == PublishStatus.Failed.ToString());
            errorMessage = string.Join("; ", failedPosts.Select(pp => $"{pp.Platform?.PlatformName}: {pp.ErrorMessage}"));
            _logger.LogWarning("Some platforms failed for CampaignPost {CampaignPostId}: {Errors}", campaignPostId, errorMessage);
        }
        else if (anyPublishing)
        {
            newStatus = PublishStatus.Publishing.ToString();
            _logger.LogInformation("CampaignPost {CampaignPostId} is still publishing to some platforms", campaignPostId);
        }
        else
        {
            // Some published, some pending - keep as Publishing or set to Pending based on majority
            var publishedCount = platformPosts.Count(pp => pp.PublishStatus == PublishStatus.Published.ToString());
            newStatus = publishedCount > 0 ? PublishStatus.Publishing.ToString() : PublishStatus.Pending.ToString();
        }

        // Only update if status actually changed to avoid unnecessary updates
        if (campaignPost.PublishStatus != newStatus || 
            (publishedAt.HasValue && campaignPost.PublishedAt != publishedAt.Value) ||
            campaignPost.LastPublishError != errorMessage)
        {
            campaignPost.PublishStatus = newStatus;
            if (publishedAt.HasValue)
            {
                campaignPost.PublishedAt = publishedAt.Value;
            }
            campaignPost.LastPublishError = errorMessage;

            await _unitOfWork.CampaignPosts.UpdateAsync(campaignPost, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated CampaignPost {CampaignPostId} status to {Status}", campaignPostId, newStatus);
        }
    }
}
