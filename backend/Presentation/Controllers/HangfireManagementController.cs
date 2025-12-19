using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Controller for managing Hangfire jobs
/// IMPORTANT: This should be secured or removed in production!
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HangfireManagementController : ControllerBase
{
    private readonly ILogger<HangfireManagementController> _logger;

    public HangfireManagementController(ILogger<HangfireManagementController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// List all recurring jobs
    /// </summary>
    [HttpGet("recurring-jobs")]
    public IActionResult GetRecurringJobs()
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            var jobList = recurringJobs.Select(job => new
            {
                job.Id,
                job.Cron,
                job.Queue,
                job.NextExecution,
                job.LastExecution,
                job.LastJobState,
                job.CreatedAt,
                Job = new
                {
                    Type = job.Job?.Type?.Name,
                    Method = job.Job?.Method?.Name,
                    Arguments = job.Job?.Args
                }
            }).ToList();

            return Ok(new
            {
                totalCount = jobList.Count,
                jobs = jobList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recurring jobs");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a specific recurring job by ID
    /// </summary>
    [HttpDelete("recurring-jobs/{jobId}")]
    public IActionResult DeleteRecurringJob(string jobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(jobId);
            _logger.LogInformation("Deleted recurring job: {JobId}", jobId);

            return Ok(new
            {
                success = true,
                message = $"Recurring job '{jobId}' deleted successfully",
                jobId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting recurring job {JobId}", jobId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Clean up legacy duplicate jobs (campaign-scheduler-job and campaign-scheduler)
    /// </summary>
    [HttpPost("cleanup-legacy-jobs")]
    public IActionResult CleanupLegacyJobs()
    {
        try
        {
            var removedJobs = new List<string>();

            // Remove the duplicate legacy jobs
            var legacyJobIds = new[] { "campaign-scheduler-job", "campaign-scheduler" };

            foreach (var jobId in legacyJobIds)
            {
                try
                {
                    RecurringJob.RemoveIfExists(jobId);
                    removedJobs.Add(jobId);
                    _logger.LogInformation("Removed legacy job: {JobId}", jobId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to remove job {JobId}", jobId);
                }
            }

            return Ok(new
            {
                success = true,
                message = "Legacy jobs cleanup completed",
                removedJobs,
                remainingJob = "platform-publisher (this is the correct one)"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during legacy jobs cleanup");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Trigger a specific recurring job immediately
    /// </summary>
    [HttpPost("trigger-job/{jobId}")]
    public IActionResult TriggerJob(string jobId)
    {
        try
        {
            RecurringJob.Trigger(jobId);
            _logger.LogInformation("Triggered job: {JobId}", jobId);

            return Ok(new
            {
                success = true,
                message = $"Job '{jobId}' triggered successfully",
                jobId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering job {JobId}", jobId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get statistics about all jobs
    /// </summary>
    [HttpGet("statistics")]
    public IActionResult GetStatistics()
    {
        try
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var stats = monitoringApi.GetStatistics();

            return Ok(new
            {
                enqueued = stats.Enqueued,
                scheduled = stats.Scheduled,
                processing = stats.Processing,
                succeeded = stats.Succeeded,
                failed = stats.Failed,
                deleted = stats.Deleted,
                recurring = stats.Recurring,
                servers = stats.Servers,
                queues = stats.Queues
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
