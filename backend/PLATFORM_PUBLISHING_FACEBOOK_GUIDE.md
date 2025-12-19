# Platform Publishing System - Facebook Integration Guide

## Overview
This document explains how the Platform Publishing System works with Facebook Pages, including where Facebook Page IDs and Access Tokens come from, and how the system schedules and publishes posts.

---

## Architecture Overview

### Publishing Flow
```
User Creates Post
    ?
Stores in CampaignPostPlatform (with ScheduledAt)
    ?
Hangfire runs PlatformPublisherJob every minute
    ?
PlatformPublishingService finds due posts (ScheduledAt <= now, Status = Pending)
    ?
Builds PublishPlatformPostRequest DTO
    ?
Routes to FacebookPublisher via PlatformName
    ?
FacebookPublisher calls Facebook Graph API
    ?
Updates CampaignPostPlatform status (Published/Failed)
```

---

## Facebook Page Connection (OAuth Flow)

### How Users Connect Their Facebook Page

1. **User Initiates Connection**
   - User clicks "Connect Facebook" in the UI
   - Frontend requests authorization URL from backend: `GET /api/social-platforms/facebook/auth-url?storeId={guid}`

2. **OAuth Authorization**
   - Backend generates Facebook OAuth URL with required permissions:
     - `pages_show_list` - View list of pages
     - `pages_manage_posts` - Publish posts to pages
     - `pages_read_engagement` - Read post insights
   - User is redirected to Facebook to authorize the app

3. **OAuth Callback**
   - Facebook redirects back with authorization code
   - Backend exchanges code for User Access Token
   - Backend fetches user's Pages and their Page Access Tokens
   - User selects which Page to connect

4. **Store Page Data**
   - Backend stores in `SocialPlatforms` table:
     - `ExternalPageID` - Facebook Page ID (e.g., "123456789")
     - `PageName` - Page display name
     - `AccessToken` - Long-lived Page Access Token (60 days validity)
     - `PlatformName` - Enum: `PlatformName.Facebook`
     - `IsConnected` - `true`

### Facebook Page ID
- **Source**: Obtained during OAuth flow from Facebook Graph API
- **Stored In**: `SocialPlatforms.ExternalPageID`
- **Format**: Numeric string (e.g., "123456789012345")
- **Usage**: Used as `/{page-id}/photos` or `/{page-id}/feed` endpoint in Graph API

### Page Access Token
- **Source**: Obtained by exchanging User Access Token for Page Access Token
- **Stored In**: `SocialPlatforms.AccessToken`
- **Validity**: 60 days (can be extended to never expire for business apps)
- **Permissions Required**:
  - `pages_manage_posts` - Publish content
  - `pages_read_engagement` - Read analytics
- **Usage**: Sent as `access_token` parameter in every Graph API request

---

## Scheduling vs Immediate Publishing

### ?? IMPORTANT: Graph API Does NOT Handle Scheduling

**Facebook Graph API Limitation:**
- The Graph API `/photos` and `/feed` endpoints publish posts **immediately**
- Facebook does NOT support native scheduling via Graph API for regular posts
- Facebook's native scheduling is only available through Meta Business Suite UI

### How This System Handles Scheduling

**Our Scheduling Implementation:**
1. User sets `ScheduledAt` time when creating a post
2. Post is stored in `CampaignPostPlatform` with `PublishStatus = Pending`
3. Hangfire runs `PlatformPublisherJob` every minute
4. When `ScheduledAt <= DateTime.UtcNow`, the post becomes "due"
5. System publishes the post **immediately to Facebook**
6. Post appears on Facebook Page at the scheduled time

**Key Points:**
- Scheduling is handled by **our application**, not Facebook
- Facebook sees each post as an immediate publish
- The scheduled time is when **our job publishes it**, not a Facebook scheduled post
- This gives us more control over retry logic, status tracking, and error handling

---

## Publishing Process (Technical Details)

### 1. Finding Due Posts

**Repository Query:**
```csharp
// In CampaignPostPlatformRepository.GetDuePlatformPostsAsync()
return await _context.Set<CampaignPostPlatform>()
    .Include(pp => pp.CampaignPost)
        .ThenInclude(cp => cp.Campaign)
    .Include(pp => pp.Platform)
    .Where(pp => 
        pp.ScheduledAt <= currentTime &&
        pp.PublishStatus == PublishStatus.Pending.ToString())
    .ToListAsync(cancellationToken);
```

**Filters:**
- ? `ScheduledAt <= currentTime` - Only past or current scheduled times
- ? `PublishStatus = Pending` - Only unpublished posts
- ? Soft-delete filter (automatic via EF Core global query filters)
- ? Store-level multi-tenancy (automatic via `IStoreContext`)

### 2. Building the Request DTO

**Application Layer:**
```csharp
var publishRequest = new PublishPlatformPostRequest
{
    Caption = platformPost.CampaignPost.PostCaption,
    ImageUrl = platformPost.CampaignPost.PostImageUrl,
    AccessToken = platformPost.Platform.AccessToken,
    ExternalPageId = platformPost.Platform.ExternalPageID,
    CampaignPostPlatformId = platformPost.Id
};
```

### 3. Publishing to Facebook

**Infrastructure Layer (FacebookPublisher):**

#### For Posts With Images:
```http
POST https://graph.facebook.com/v18.0/{page-id}/photos

Body (form-urlencoded):
message: {Caption}
url: {ImageUrl}
access_token: {AccessToken}
```

**Response:**
```json
{
  "id": "page-id_photo-id",
  "post_id": "page-id_post-id"
}
```

#### For Text-Only Posts:
```http
POST https://graph.facebook.com/v18.0/{page-id}/feed

Body (form-urlencoded):
message: {Caption}
access_token: {AccessToken}
```

**Response:**
```json
{
  "id": "page-id_post-id"
}
```

### 4. Storing External Post ID

**After Successful Publishing:**
```csharp
platformPost.ExternalPostId = result.ExternalPostId; // Facebook's post ID
platformPost.PublishStatus = PublishStatus.Published.ToString();
platformPost.PublishedAt = DateTime.UtcNow;
platformPost.ErrorMessage = null;
await _platformPostRepository.UpdateAsync(platformPost, cancellationToken);
```

**External Post ID Format:**
- Example: `"123456789_987654321"`
- Format: `"{page-id}_{post-id}"`
- Can be used to fetch post analytics later
- Can be used to edit/delete the post if needed

---

## Error Handling

### Facebook API Errors

**Common Errors:**

1. **Invalid Access Token (Code 190)**
   ```json
   {
     "error": {
       "message": "(#190) Invalid OAuth 2.0 Access Token",
       "type": "OAuthException",
       "code": 190
     }
   }
   ```
   **Solution**: User needs to reconnect their Facebook Page

2. **Page Not Found (Code 803)**
   ```json
   {
     "error": {
       "message": "(#803) Some of the aliases you requested do not exist",
       "type": "OAuthException",
       "code": 803
     }
   }
   ```
   **Solution**: Page ID is invalid or page was deleted

3. **Rate Limit (Code 4)**
   ```json
   {
     "error": {
       "message": "(#4) Application request limit reached",
       "type": "OAuthException",
       "code": 4
     }
   }
   ```
   **Solution**: Retry after cool-down period

### Our Error Handling

**Infrastructure Layer:**
```csharp
// FacebookPublisher returns structured results (never throws)
return PublishPlatformPostResult.Failure($"Facebook API error: {errorContent}");
```

**Application Layer:**
```csharp
if (result.IsSuccess)
{
    // Mark as Published
}
else
{
    // Mark as Failed with error message
    await MarkAsFailed(platformPost, result.ErrorMessage ?? "Unknown error", cancellationToken);
}
```

**Stored Error Messages:**
- Stored in `CampaignPostPlatform.ErrorMessage`
- Visible in Hangfire dashboard logs
- Can be displayed to users for troubleshooting

---

## Status Flow Diagram

```
???????????????????????????????????????????
? User Creates Post                       ?
? ScheduledAt: 2024-06-15 10:00:00 UTC  ?
? Status: Pending                         ?
???????????????????????????????????????????
                  ?
???????????????????????????????????????????
? Hangfire PlatformPublisherJob           ?
? Runs every minute                       ?
? Checks: ScheduledAt <= DateTime.UtcNow  ?
???????????????????????????????????????????
                  ?
          ?????????????????
          ?               ?
      [Due Posts]     [Not Yet Due]
          ?               ?
          ?               ?
???????????????????   [Skip]
? Status: Publishing?
???????????????????
          ?
???????????????????????????????????????????
? PlatformPublishingService               ?
? - Validates Campaign active             ?
? - Selects Publisher (FacebookPublisher) ?
? - Builds PublishPlatformPostRequest     ?
???????????????????????????????????????????
          ?
???????????????????????????????????????????
? FacebookPublisher.PublishAsync()        ?
? POST /{page-id}/photos or /feed         ?
? Returns: PublishPlatformPostResult      ?
???????????????????????????????????????????
          ?
    ?????????????
    ?           ?
[Success]   [Failure]
    ?           ?
    ?           ?
??????????? ???????????
?Status:  ? ?Status:  ?
?Published? ?Failed   ?
?         ? ?Error:   ?
?External ? ?"OAuth..."?
?PostId   ? ?         ?
??????????? ???????????
```

---

## Database Schema

### SocialPlatforms Table
```sql
CREATE TABLE SocialPlatforms (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    StoreId UNIQUEIDENTIFIER NOT NULL,
    PlatformName NVARCHAR(50) NOT NULL, -- "Facebook"
    ExternalPageID NVARCHAR(200) NOT NULL, -- "123456789012345"
    PageName NVARCHAR(200) NOT NULL, -- "My Business Page"
    AccessToken NVARCHAR(2000) NOT NULL, -- Long-lived Page Access Token
    IsConnected BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL
);
```

### CampaignPostPlatforms Table
```sql
CREATE TABLE CampaignPostPlatforms (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CampaignPostId UNIQUEIDENTIFIER NOT NULL,
    PlatformId UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId NVARCHAR(200) NULL, -- Facebook's post ID
    PublishStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ScheduledAt DATETIME2 NOT NULL,
    PublishedAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    FOREIGN KEY (CampaignPostId) REFERENCES CampaignPosts(Id),
    FOREIGN KEY (PlatformId) REFERENCES SocialPlatforms(Id)
);
```

---

## Testing & Verification

### Manual Testing

1. **Connect Facebook Page**
   ```http
   GET /api/social-platforms/facebook/auth-url?storeId={guid}
   ```
   - Complete OAuth flow
   - Verify Page ID and Access Token are stored

2. **Create Scheduled Post**
   ```http
   POST /api/campaign-posts
   {
     "campaignId": "guid",
     "postCaption": "Test post",
     "postImageUrl": "https://example.com/image.jpg",
     "scheduledAt": "2024-06-15T10:00:00Z"
   }
   ```

3. **Verify CampaignPostPlatform Created**
   ```sql
   SELECT * FROM CampaignPostPlatforms 
   WHERE CampaignPostId = '...'
   ```

4. **Trigger Hangfire Job Manually**
   - Go to `/hangfire` dashboard
   - Find `platform-publisher` job
   - Click "Trigger now"

5. **Verify Post on Facebook**
   - Check Facebook Page
   - Verify post content matches
   - Verify image appears (if provided)

6. **Check External Post ID**
   ```sql
   SELECT ExternalPostId, PublishStatus, ErrorMessage
   FROM CampaignPostPlatforms 
   WHERE Id = '...'
   ```

### Hangfire Dashboard

**Monitor Jobs:**
- URL: `https://localhost:5001/hangfire`
- Job: `platform-publisher`
- Schedule: `* * * * *` (every minute)

**Check Logs:**
- View execution history
- See success/failure counts
- Inspect error messages

---

## Security Considerations

### Access Token Storage
- ? Stored encrypted at rest (if encryption enabled in SQL Server)
- ?? Consider using Azure Key Vault or similar for production
- ?? Tokens expire after 60 days - implement refresh mechanism

### Multi-Tenancy
- ? Store-level isolation via `IStoreContext`
- ? Users can only publish to their own connected pages
- ? Authorization handled by `StoreValidationMiddleware`

### Rate Limiting
- ?? Facebook has rate limits per App and per Page
- ?? Consider implementing exponential backoff for retries
- ?? Monitor usage in Facebook App Dashboard

---

## Future Enhancements

### 1. Token Refresh
- Implement automatic token renewal before expiration
- Store token expiry date
- Refresh tokens proactively

### 2. Analytics Integration
- Fetch post insights from Facebook
- Store engagement metrics (likes, comments, shares)
- Display analytics in dashboard

### 3. Post Editing/Deletion
- Use `ExternalPostId` to edit posts via Graph API
- Allow users to update or remove published posts

### 4. Media Upload Optimization
- Upload images to Facebook first, get media ID
- Use media ID for more reliable photo posts
- Support video uploads

### 5. Instagram Integration
- Similar flow using Instagram Graph API
- Share posts to both Facebook and Instagram
- Handle Instagram-specific requirements (hashtags, @mentions)

---

## Troubleshooting

### Post Not Publishing

1. **Check Hangfire Dashboard**
   - Is `platform-publisher` job running?
   - Any errors in execution history?

2. **Verify Post Status**
   ```sql
   SELECT PublishStatus, ScheduledAt, ErrorMessage
   FROM CampaignPostPlatforms 
   WHERE CampaignPostId = '...'
   ```

3. **Check Campaign Settings**
   ```sql
   SELECT IsSchedulingEnabled, ScheduledStartAt, ScheduledEndAt
   FROM Campaigns 
   WHERE Id = '...'
   ```

4. **Verify Platform Connection**
   ```sql
   SELECT IsConnected, AccessToken
   FROM SocialPlatforms 
   WHERE Id = '...'
   ```

### Invalid Access Token

**Symptoms:**
- Error: "(#190) Invalid OAuth 2.0 Access Token"
- Status: Failed

**Solution:**
1. User must reconnect Facebook Page
2. Go through OAuth flow again
3. New token will be stored

### Page Not Found

**Symptoms:**
- Error: "(#803) Page aliases do not exist"
- Status: Failed

**Solution:**
1. Verify Page ID is correct
2. Check if Page was deleted on Facebook
3. Reconnect the correct Page

---

## References

- [Facebook Graph API Documentation](https://developers.facebook.com/docs/graph-api/)
- [Facebook Page Publishing](https://developers.facebook.com/docs/pages/publishing/)
- [Facebook OAuth Reference](https://developers.facebook.com/docs/facebook-login/guides/access-tokens/)
- [Facebook Error Codes](https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling/)

---

**Last Updated**: December 2024  
**Version**: 1.0  
**System**: Business Manager Platform Publishing  
**Framework**: .NET 8 Clean Architecture
