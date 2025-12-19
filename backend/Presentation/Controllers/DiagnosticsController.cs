using Application.Services.Publishing;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Interfaces;
using Hangfire;

namespace Presentation.Controllers;

/// <summary>
/// Diagnostic endpoints for testing and debugging scheduling system
/// IMPORTANT: Remove or secure these endpoints in production!
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IPlatformPublishingService _platformPublishingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IPlatformPublishingService platformPublishingService,
        IUnitOfWork unitOfWork,
        ILogger<DiagnosticsController> logger)
    {
        _platformPublishingService = platformPublishingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Manually trigger the platform publishing job
    /// </summary>
    [HttpPost("trigger-platform-publisher")]
    public async Task<IActionResult> TriggerPlatformPublisher()
    {
        try
        {
            _logger.LogInformation("Manually triggering platform publisher job");
            await _platformPublishingService.ProcessDuePlatformPostsAsync();
            return Ok(new { message = "Platform publisher job executed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing platform publisher job");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get all pending CampaignPostPlatform records to see what's scheduled
    /// </summary>
    [HttpGet("pending-platform-posts")]
    public async Task<IActionResult> GetPendingPlatformPosts()
    {
        try
        {
            var now = DateTime.UtcNow;
            var duePosts = await _unitOfWork.CampaignPostPlatforms.GetDuePlatformPostsAsync(now);
            
            var result = duePosts.Select(pp => new
            {
                pp.Id,
                CampaignPostId = pp.CampaignPostId,
                PlatformId = pp.PlatformId,
                PlatformName = pp.Platform?.PlatformName.ToString() ?? "Unknown",
                pp.ScheduledAt,
                pp.PublishStatus,
                MinutesPastDue = (now - pp.ScheduledAt).TotalMinutes,
                Campaign = new
                {
                    pp.CampaignPost.Campaign.Id,
                    Name = pp.CampaignPost.Campaign.CampaignName,
                    pp.CampaignPost.Campaign.IsSchedulingEnabled,
                    pp.CampaignPost.Campaign.ScheduledStartAt,
                    pp.CampaignPost.Campaign.ScheduledEndAt,
                    Stage = pp.CampaignPost.Campaign.CampaignStage.ToString()
                },
                Post = new
                {
                    Caption = pp.CampaignPost?.PostCaption,
                    ImageUrl = pp.CampaignPost?.PostImageUrl
                }
            }).ToList();

            return Ok(new
            {
                currentTime = now,
                duePostsCount = result.Count,
                duePosts = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending platform posts");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get detailed analysis of ALL pending posts (regardless of due time)
    /// </summary>
    [HttpGet("analyze-pending-posts")]
    public async Task<IActionResult> AnalyzePendingPosts()
    {
        try
        {
            var now = DateTime.UtcNow;
            
            // Get all pending posts (not filtered by time)
            var allPosts = await _unitOfWork.CampaignPostPlatforms.GetAllAsync();
            var pendingPosts = allPosts.Where(pp => pp.PublishStatus == "Pending").ToList();
            
            var analysis = pendingPosts.Select(pp => new
            {
                pp.Id,
                pp.ScheduledAt,
                pp.PublishStatus,
                CurrentTimeUtc = now,
                ScheduledTimeUtc = pp.ScheduledAt,
                IsDue = pp.ScheduledAt <= now,
                MinutesUntilDue = (pp.ScheduledAt - now).TotalMinutes,
                TimeDifference = pp.ScheduledAt - now,
                Campaign = new
                {
                    pp.CampaignPost.Campaign.Id,
                    Name = pp.CampaignPost.Campaign.CampaignName,
                    pp.CampaignPost.Campaign.IsSchedulingEnabled,
                    pp.CampaignPost.Campaign.ScheduledStartAt,
                    pp.CampaignPost.Campaign.ScheduledEndAt,
                    IsWithinScheduleWindow = (!pp.CampaignPost.Campaign.ScheduledStartAt.HasValue || now >= pp.CampaignPost.Campaign.ScheduledStartAt.Value) &&
                                            (!pp.CampaignPost.Campaign.ScheduledEndAt.HasValue || now <= pp.CampaignPost.Campaign.ScheduledEndAt.Value)
                },
                Platform = new
                {
                    pp.Platform.Id,
                    pp.Platform.PlatformName,
                    pp.Platform.IsConnected
                },
                BlockingReasons = GetBlockingReasons(pp, now)
            }).ToList();

            return Ok(new
            {
                currentTime = now,
                serverTimeZone = TimeZoneInfo.Local.DisplayName,
                totalPendingCount = analysis.Count,
                dueCount = analysis.Count(a => a.IsDue),
                notDueYetCount = analysis.Count(a => !a.IsDue),
                posts = analysis
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing pending posts");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    private List<string> GetBlockingReasons(Domain.Entities.CampaignPostPlatform platformPost, DateTime now)
    {
        var reasons = new List<string>();

        // Check if scheduled time is in the future
        if (platformPost.ScheduledAt > now)
        {
            reasons.Add($"Scheduled for future: {(platformPost.ScheduledAt - now).TotalMinutes:F1} minutes from now");
        }

        // Check campaign scheduling
        if (!platformPost.CampaignPost.Campaign.IsSchedulingEnabled)
        {
            reasons.Add("Campaign scheduling is disabled");
        }

        // Check campaign start time
        if (platformPost.CampaignPost.Campaign.ScheduledStartAt.HasValue && 
            now < platformPost.CampaignPost.Campaign.ScheduledStartAt.Value)
        {
            reasons.Add($"Campaign hasn't started yet (starts at {platformPost.CampaignPost.Campaign.ScheduledStartAt.Value})");
        }

        // Check campaign end time
        if (platformPost.CampaignPost.Campaign.ScheduledEndAt.HasValue && 
            now > platformPost.CampaignPost.Campaign.ScheduledEndAt.Value)
        {
            reasons.Add($"Campaign has ended (ended at {platformPost.CampaignPost.Campaign.ScheduledEndAt.Value})");
        }

        // Check platform connection
        if (!platformPost.Platform.IsConnected)
        {
            reasons.Add("Platform is not connected");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("No blocking reasons - should be publishable");
        }

        return reasons;
    }

    /// <summary>
    /// Get all CampaignPostPlatform records (not just due ones)
    /// </summary>
    [HttpGet("all-platform-posts")]
    public async Task<IActionResult> GetAllPlatformPosts()
    {
        try
        {
            var now = DateTime.UtcNow;
            var allPosts = await _unitOfWork.CampaignPostPlatforms.GetAllAsync();
            
            var result = allPosts.Select(pp => new
            {
                pp.Id,
                CampaignPostId = pp.CampaignPostId,
                PlatformId = pp.PlatformId,
                pp.ScheduledAt,
                pp.PublishStatus,
                pp.PublishedAt,
                pp.ExternalPostId,
                pp.ErrorMessage,
                IsDue = pp.ScheduledAt <= now,
                MinutesUntilDue = (pp.ScheduledAt - now).TotalMinutes
            }).ToList();

            return Ok(new
            {
                currentTime = now,
                totalCount = result.Count,
                pendingCount = result.Count(p => p.PublishStatus == "Pending"),
                publishedCount = result.Count(p => p.PublishStatus == "Published"),
                failedCount = result.Count(p => p.PublishStatus == "Failed"),
                dueCount = result.Count(p => p.IsDue && p.PublishStatus == "Pending"),
                platformPosts = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all platform posts");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Get Hangfire job status
    /// </summary>
    [HttpGet("hangfire-status")]
    public IActionResult GetHangfireStatus()
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var stats = monitoringApi.GetStatistics();
            var servers = monitoringApi.Servers();
            
            return Ok(new
            {
                hangfireConnected = true,
                serversCount = servers.Count,
                enqueued = stats.Enqueued,
                scheduled = stats.Scheduled,
                processing = stats.Processing,
                succeeded = stats.Succeeded,
                failed = stats.Failed,
                recurring = stats.Recurring,
                servers = servers.Select(s => new
                {
                    s.Name,
                    s.Queues,
                    s.StartedAt,
                    s.Heartbeat
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Hangfire status");
            return StatusCode(500, new { error = ex.Message, hangfireConnected = false });
        }
    }

    /// <summary>
    /// Check connected social platforms for a store
    /// </summary>
    [HttpGet("connected-platforms/{storeId}")]
    public async Task<IActionResult> GetConnectedPlatforms(Guid storeId)
    {
        try
        {
            var platforms = await _unitOfWork.SocialPlatforms.GetConnectedPlatformsByStoreIdAsync(storeId);
            
            var result = platforms.Select(p => new
            {
                p.Id,
                p.PlatformName,
                p.IsConnected,
                p.ExternalPageID,
                p.PageName,
                HasAccessToken = !string.IsNullOrEmpty(p.AccessToken),
                p.UpdatedAt
            }).ToList();

            return Ok(new
            {
                storeId,
                connectedPlatformsCount = result.Count,
                platforms = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connected platforms");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get status comparison between CampaignPost and its CampaignPostPlatforms
    /// Useful for debugging status sync issues
    /// </summary>
    [HttpGet("status-sync/{campaignPostId}")]
    public async Task<IActionResult> GetStatusSync(Guid campaignPostId)
    {
        try
        {
            var campaignPost = await _unitOfWork.CampaignPosts.GetByIdAsync(campaignPostId);
            if (campaignPost == null)
            {
                return NotFound(new { error = $"CampaignPost {campaignPostId} not found" });
            }

            var platformPosts = await _unitOfWork.CampaignPostPlatforms.GetByCampaignPostIdAsync(campaignPostId);

            var platformStatuses = platformPosts.Select(pp => new
            {
                pp.Id,
                Platform = pp.Platform?.PlatformName.ToString() ?? "Unknown",
                pp.PublishStatus,
                pp.ScheduledAt,
                pp.PublishedAt,
                pp.ExternalPostId,
                pp.ErrorMessage
            }).ToList();

            return Ok(new
            {
                campaignPost = new
                {
                    campaignPost.Id,
                    campaignPost.PostCaption,
                    campaignPost.PublishStatus,
                    campaignPost.ScheduledAt,
                    campaignPost.PublishedAt,
                    campaignPost.LastPublishError
                },
                platformPostsCount = platformStatuses.Count,
                platformPosts = platformStatuses,
                statusAnalysis = new
                {
                    allPublished = platformStatuses.All(p => p.PublishStatus == "Published"),
                    anyFailed = platformStatuses.Any(p => p.PublishStatus == "Failed"),
                    anyPublishing = platformStatuses.Any(p => p.PublishStatus == "Publishing"),
                    pendingCount = platformStatuses.Count(p => p.PublishStatus == "Pending"),
                    publishedCount = platformStatuses.Count(p => p.PublishStatus == "Published"),
                    failedCount = platformStatuses.Count(p => p.PublishStatus == "Failed"),
                    publishingCount = platformStatuses.Count(p => p.PublishStatus == "Publishing"),
                    expectedParentStatus = GetExpectedStatus(platformStatuses.Select(p => p.PublishStatus).ToList())
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status sync for campaign post {CampaignPostId}", campaignPostId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private string GetExpectedStatus(List<string> platformStatuses)
    {
        if (platformStatuses.All(s => s == "Published"))
            return "Published";
        
        if (platformStatuses.Any(s => s == "Failed") && !platformStatuses.Any(s => s == "Publishing"))
            return "Failed";
        
        if (platformStatuses.Any(s => s == "Publishing"))
            return "Publishing";
        
        if (platformStatuses.Count(s => s == "Published") > 0)
            return "Publishing";
        
        return "Pending";
    }

    /// <summary>
    /// Check for duplicate ExternalPostIds (indicates duplicate publishing)
    /// </summary>
    [HttpGet("check-duplicates")]
    public async Task<IActionResult> CheckForDuplicates()
    {
        try
        {
            var allPosts = await _unitOfWork.CampaignPostPlatforms.GetAllAsync();
            
            // Group by ExternalPostId to find duplicates
            var duplicateGroups = allPosts
                .Where(pp => !string.IsNullOrEmpty(pp.ExternalPostId))
                .GroupBy(pp => pp.ExternalPostId)
                .Where(g => g.Count() > 1)
                .ToList();

            var duplicates = duplicateGroups.Select(g => new
            {
                externalPostId = g.Key,
                occurrences = g.Count(),
                posts = g.Select(pp => new
                {
                    pp.Id,
                    pp.CampaignPostId,
                    Platform = pp.Platform?.PlatformName.ToString() ?? "Unknown",
                    pp.PublishStatus,
                    pp.PublishedAt,
                    Caption = pp.CampaignPost?.PostCaption
                }).ToList()
            }).ToList();

            return Ok(new
            {
                hasDuplicates = duplicates.Any(),
                duplicateCount = duplicates.Count,
                totalDuplicatePosts = duplicates.Sum(d => d.occurrences),
                duplicates = duplicates,
                message = duplicates.Any() 
                    ? "?? Duplicate external post IDs found - same post published multiple times!" 
                    : "? No duplicates found - each post published exactly once"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicates");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
