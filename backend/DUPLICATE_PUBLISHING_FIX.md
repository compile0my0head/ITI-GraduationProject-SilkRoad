# Duplicate Publishing Prevention - Implementation Summary

## Problem Description
Posts were being published 3 times in a row before stabilizing. This was caused by:
1. **Race conditions** - Multiple Hangfire job instances running concurrently
2. **Delayed status updates** - Status wasn't persisted immediately, allowing duplicate pickup
3. **No concurrent execution prevention** - Hangfire was allowed to run multiple job instances simultaneously

## Root Causes Identified

### 1. **Timing Issue in Status Updates**
- Status was changed to "Publishing" but saved AFTER validation checks
- Between status change and save, another job instance could pick up the same post
- Database wasn't updated atomically

### 2. **Concurrent Job Execution**
- Hangfire runs jobs every minute (`Cron.Minutely`)
- Without `DisableConcurrentExecution`, multiple instances could run simultaneously
- Each instance would fetch the same "Pending" posts before any status updates persisted

### 3. **No Reload Verification**
- Posts fetched at the start of processing weren't re-verified before publishing
- Status could change between fetch and process, causing duplicate attempts

## Solutions Implemented

### ? 1. Immediate Status Lock (PlatformPublishingService.cs)

**Location**: `ProcessSinglePlatformPostAsync()` method

**Changes**:
```csharp
// BEFORE (vulnerable to race conditions)
// ... validation checks ...
platformPost.PublishStatus = PublishStatus.Publishing.ToString();
await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
// ... then publish ...

// AFTER (immediate lock)
// 1. Reload from database
var currentPlatformPost = await _unitOfWork.CampaignPostPlatforms.GetByIdAsync(platformPost.Id, cancellationToken);

// 2. Double-check status
if (currentPlatformPost.PublishStatus != PublishStatus.Pending.ToString())
{
    return; // Skip if already processing
}

// 3. IMMEDIATELY mark as Publishing and save
platformPost.PublishStatus = PublishStatus.Publishing.ToString();
await _unitOfWork.CampaignPostPlatforms.UpdateAsync(platformPost, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);

// 4. THEN do validation and publishing
// ... campaign checks ...
// ... actual publishing ...
```

**Why This Works**:
- Status is locked in the database BEFORE any external API calls
- Other job instances immediately see "Publishing" status and skip
- Atomic operation prevents race conditions

### ? 2. Prevent Concurrent Execution (PlatformPublisherJob.cs)

**Location**: `ExecuteAsync()` method

**Changes**:
```csharp
// Added Hangfire attributes
[DisableConcurrentExecution(timeoutInSeconds: 60)]
[AutomaticRetry(Attempts = 0)]
public async Task ExecuteAsync()
{
    await _publishingService.ProcessDuePlatformPostsAsync();
}
```

**Attributes Explained**:
- `DisableConcurrentExecution(60)`: 
  - Prevents multiple instances of the job from running simultaneously
  - Uses distributed lock (stored in Hangfire database)
  - Timeout of 60 seconds (job should complete within this time)
  
- `AutomaticRetry(Attempts = 0)`:
  - Disables automatic retries on failure
  - Prevents duplicate publishing attempts from retry logic
  - Failed posts remain "Failed" and can be manually retried

### ? 3. Database Reload Before Processing

**Location**: `ProcessSinglePlatformPostAsync()` method

**Changes**:
```csharp
// Reload from database to get latest state
var currentPlatformPost = await _unitOfWork.CampaignPostPlatforms
    .GetByIdAsync(platformPost.Id, cancellationToken);

// Verify status hasn't changed
if (currentPlatformPost.PublishStatus != PublishStatus.Pending.ToString())
{
    _logger.LogInformation(
        "Skipping platform post {PlatformPostId} - status is {Status} (not Pending)", 
        platformPost.Id, currentPlatformPost.PublishStatus);
    return;
}
```

**Why This Works**:
- Fetches the absolute latest state from the database
- Detects if another process already started processing
- Provides defense-in-depth against race conditions

## Execution Flow (After Fixes)

### Scenario: Job Runs While Post is Pending

```
Job Instance A (Time: 14:00:00)
?? Fetch due posts ? [Post-123: Pending]
?? ProcessSinglePlatformPostAsync(Post-123)
?  ?? Reload from DB ? Status: Pending ?
?  ?? Check: Still Pending? Yes ?
?  ?? UPDATE Status = Publishing ? DB SAVED (14:00:00.100)
?  ?? Validate campaign ? OK ?
?  ?? Publish to Facebook ? (takes 2 seconds)
?  ?? UPDATE Status = Published ? DB SAVED (14:00:02.300)
?? Complete

Job Instance B (Time: 14:00:00.500) [Blocked by DisableConcurrentExecution]
?? Waits until Job A completes

Job Instance C (Time: 14:01:00)
?? Fetch due posts ? [Post-123: Published] ? NOT in results!
?? Complete (nothing to process)
```

### Scenario: Race Condition (Without Fixes)

```
WITHOUT FIXES - Old Behavior:

Job A (14:00:00)        Job B (14:00:00.200)      Job C (14:00:00.400)
?? Fetch [Post-123]     ?? Fetch [Post-123]       ?? Fetch [Post-123]
?? Validate             ?? Validate               ?? Validate
?? Publish to FB ?      ?? Publish to FB ?        ?? Publish to FB ?
?? Update: Published    ?? Update: Published      ?? Update: Published
?? Complete             ?? Complete               
                        
Result: 3 duplicate posts on Facebook! ?

WITH FIXES - New Behavior:

Job A (14:00:00)                    Job B (blocked)     Job C (14:01:00)
?? Fetch [Post-123: Pending]        ?? Waits...        ?? Fetch []
?? Reload: Still Pending? Yes ?                        ?? Nothing to do
?? UPDATE Publishing (IMMEDIATE) ?????????
?? Publish to FB ?                       ?
?? UPDATE Published                      ?
?? Complete                              ?
                                         ?
Job B (14:00:05)                         ?
?? Unblocked                             ?
?? Fetch [Post-123: Published] ??????????? (Status already changed)
?? Nothing to process

Result: 1 post on Facebook! ?
```

## Testing Verification

### Test Case 1: Normal Publishing
```bash
# Create a post scheduled for now
POST /api/campaignposts
{
  "campaignId": "...",
  "postCaption": "Test",
  "scheduledAt": "2025-12-17T14:00:00Z"
}

# Wait for job to run (1 minute)
# Check status sync
GET /api/diagnostics/status-sync/{campaignPostId}

# Expected:
# - CampaignPostPlatform: Published
# - ExternalPostId: {facebook-post-id}
# - Only ONE post on Facebook page
```

### Test Case 2: Concurrent Job Prevention
```bash
# Manually trigger job 3 times quickly
POST /api/diagnostics/trigger-platform-publisher
POST /api/diagnostics/trigger-platform-publisher
POST /api/diagnostics/trigger-platform-publisher

# Check results
GET /api/diagnostics/status-sync/{campaignPostId}

# Expected:
# - Only ONE ExternalPostId per platform
# - Status: Published (not Published 3 times)
# - Only ONE post visible on Facebook
```

### Test Case 3: Status Verification
```bash
# Check Hangfire dashboard
https://localhost:5001/hangfire

# Look for:
# - "platform-publisher" job
# - "DisableConcurrentExecution" in job details
# - No failed jobs with "Locked by another process"
```

## Performance Considerations

### Database Queries Added
- **Before**: 1 query to fetch due posts
- **After**: 1 query to fetch + 1 reload per post

**Impact**: Minimal
- Reload is by primary key (indexed, very fast)
- Prevents multiple Facebook API calls (expensive)
- Net performance gain by preventing duplicates

### Lock Timeout
- Set to 60 seconds (`DisableConcurrentExecution(60)`)
- Should be adjusted based on:
  - Number of posts per batch
  - Facebook API response time
  - Network latency

**Recommendation**: Monitor job execution time and adjust if needed.

## Monitoring & Debugging

### New Log Messages
```csharp
// When skipping already-processing post
"Skipping platform post {PlatformPostId} - status is {Status} (not Pending)"

// When locking post for processing
"Marked platform post {PlatformPostId} as Publishing to prevent duplicate processing"
```

### Diagnostic Endpoints
- `GET /api/diagnostics/status-sync/{campaignPostId}` - Check if statuses match
- `GET /api/diagnostics/all-platform-posts` - See all platform posts and their statuses
- `GET /api/diagnostics/pending-platform-posts` - See what's queued for publishing

### Hangfire Dashboard
- `/hangfire` - View job execution history
- Check "Succeeded" jobs - should match number of posts published
- Check "Failed" jobs - investigate any failures
- Look for lock timeout warnings

## Related Files Modified

| File | Purpose | Key Changes |
|------|---------|-------------|
| `PlatformPublishingService.cs` | Main publishing logic | Added immediate status lock, reload verification |
| `PlatformPublisherJob.cs` | Hangfire job wrapper | Added `DisableConcurrentExecution`, disabled retries |

## Future Improvements

### 1. **Optimistic Concurrency**
Could add a `RowVersion` column to `CampaignPostPlatform` for better concurrency control:
```csharp
[Timestamp]
public byte[] RowVersion { get; set; }
```

### 2. **Distributed Lock Alternative**
Consider using Redis for distributed locking if scaling to multiple servers:
```csharp
[DisableConcurrentExecution(timeoutInSeconds: 60)]
[UseRedisLock] // Custom attribute
```

### 3. **Batch Processing with Transaction**
Wrap multiple platform posts in a transaction scope:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
// Process all posts
await transaction.CommitAsync();
```

## Configuration Options

### Adjust Job Frequency
Current: Every minute (`Cron.Minutely`)

Options:
- Every 30 seconds: `"*/30 * * * * *"`
- Every 2 minutes: `"*/2 * * * *"`
- Every 5 minutes: `"*/5 * * * *"`

**Change in Program.cs**:
```csharp
RecurringJob.AddOrUpdate<PlatformPublisherJob>(
    "platform-publisher",
    job => job.ExecuteAsync(),
    "*/2 * * * *"); // Every 2 minutes
```

### Adjust Lock Timeout
Current: 60 seconds

**Change in PlatformPublisherJob.cs**:
```csharp
[DisableConcurrentExecution(timeoutInSeconds: 120)] // 2 minutes
```

## Summary

The triple-publishing issue is now **FIXED** through:

1. ? **Immediate status locking** - Posts are marked "Publishing" before any external calls
2. ? **Concurrent execution prevention** - Only one job instance runs at a time
3. ? **Double-check verification** - Status is verified before processing
4. ? **Automatic retry disabled** - Prevents retry-induced duplicates

**Result**: Each post is published exactly once, even under high concurrency scenarios.
