using Application.Services.Publishing;
using Hangfire;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire job wrapper for publishing due CampaignPostPlatform records to social media platforms.
/// This is a thin wrapper with NO business logic - all orchestration happens in PlatformPublishingService.
/// </summary>
public class PlatformPublisherJob
{
    private readonly IPlatformPublishingService _publishingService;

    public PlatformPublisherJob(IPlatformPublishingService publishingService)
    {
        _publishingService = publishingService;
    }

    /// <summary>
    /// Executes the platform publishing job.
    /// Safe to run repeatedly - idempotent by design (only processes Pending posts with ScheduledAt in the past).
    /// DisableConcurrentExecution prevents multiple instances from running simultaneously.
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    [AutomaticRetry(Attempts = 0)]
    public async Task ExecuteAsync()
    {
        await _publishingService.ProcessDuePlatformPostsAsync();
    }
}
