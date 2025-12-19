# Facebook Graph API Integration Guide

## ?? Overview

This guide explains how to complete the Facebook Graph API integration for automated post publishing in the Business Manager platform. The basic infrastructure is already in place - you need to set up the Facebook App and configure OAuth flows.

---

## ?? Current Implementation Status

### ? What's Already Done
- ? `FacebookPublisher.cs` service implementation
- ? Facebook Graph API v18.0 integration
- ? Support for text and photo posts
- ? Error handling and logging
- ? Hangfire background job scheduling
- ? `CampaignSchedulingService` orchestration
- ? Database schema for social platforms
- ? PublishStatus enum usage throughout

### ?? What Needs Configuration
- ?? Facebook App creation and configuration
- ?? OAuth 2.0 callback implementation
- ?? Page access token acquisition
- ?? Testing with real Facebook pages

---

## ?? Files Requiring Facebook App Configuration

### 1. **`Presentation/appsettings.json`** ?
**Current Configuration:**
```json
{
  "Facebook": {
    "AppId": "866146375880082",
    "AppSecret": "c12747b78f52ec735a7be696e981c48e",
    "RedirectUri": "https://localhost:5001/api/social-platforms/facebook/callback"
  }
}
```

**What You Need:**
- Replace `AppId` with your Facebook App ID
- Replace `AppSecret` with your Facebook App Secret
- Update `RedirectUri` to match your domain (localhost for dev, production URL for prod)

---

### 2. **`Infrastructure/Services/FacebookPublisher.cs`** ?
**Purpose**: Publishes posts to Facebook Pages using Graph API

**Endpoints Used:**
```csharp
// For posts with images
POST https://graph.facebook.com/v18.0/{page-id}/photos
Parameters:
  - message: Post caption
  - url: Image URL
  - access_token: Page access token

// For text-only posts
POST https://graph.facebook.com/v18.0/{page-id}/feed
Parameters:
  - message: Post caption
  - access_token: Page access token
```

**What's Already Implemented:**
- HTTP client setup
- Photo and text post handling
- Error handling with detailed logging
- Response parsing

**What You Need to Test:**
- Image URL accessibility from Facebook servers
- Page access token validity
- Graph API version compatibility

---

### 3. **`Application/Services/SocialPlatformService.cs`** ??
**Purpose**: Manages social platform connections

**Current Status:**
- Connection flow partially implemented
- Stores platform details in database

**What Needs Implementation:**

#### OAuth Callback Handler
```csharp
// File: Application/Services/SocialPlatformService.cs
// Method to add: HandleFacebookCallbackAsync

public async Task<SocialPlatformDto> HandleFacebookCallbackAsync(
    string code, 
    Guid storeId, 
    CancellationToken cancellationToken = default)
{
    // 1. Exchange code for user access token
    var userAccessToken = await ExchangeCodeForAccessTokenAsync(code);
    
    // 2. Get user's Facebook pages
    var pages = await GetUserPagesAsync(userAccessToken);
    
    // 3. For selected page, get long-lived page access token
    var pageAccessToken = await GetPageAccessTokenAsync(userAccessToken, pageId);
    
    // 4. Store page details in database
    var platform = new SocialPlatform
    {
        StoreId = storeId,
        PlatformName = PlatformName.Facebook,
        ExternalPageID = pageId,
        PageName = pageName,
        AccessToken = pageAccessToken,
        IsConnected = true
    };
    
    await _unitOfWork.SocialPlatforms.AddAsync(platform, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return _mapper.Map<SocialPlatformDto>(platform);
}
```

---

### 4. **`Presentation/Controllers/SocialPlatformController.cs`** ??
**Purpose**: Handles Facebook OAuth callbacks

**Endpoints Needed:**

#### A. Get Facebook Authorization URL
```csharp
[HttpGet("facebook/auth-url")]
public IActionResult GetFacebookAuthUrl([FromQuery] Guid storeId)
{
    var appId = _configuration["Facebook:AppId"];
    var redirectUri = _configuration["Facebook:RedirectUri"];
    var state = GenerateState(storeId); // Include storeId for callback
    
    var authUrl = $"https://www.facebook.com/v18.0/dialog/oauth?" +
                  $"client_id={appId}" +
                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                  $"&state={state}" +
                  $"&scope=pages_show_list,pages_read_engagement,pages_manage_posts";
    
    return Ok(new { authUrl });
}
```

#### B. Handle Facebook Callback
```csharp
[HttpGet("facebook/callback")]
public async Task<IActionResult> FacebookCallback(
    [FromQuery] string code, 
    [FromQuery] string state)
{
    if (string.IsNullOrEmpty(code))
    {
        return BadRequest("Authorization code is required");
    }
    
    // Extract storeId from state
    var storeId = ExtractStoreIdFromState(state);
    
    // Handle OAuth callback
    var platform = await _socialPlatformService.HandleFacebookCallbackAsync(
        code, storeId);
    
    // Redirect to frontend success page
    return Redirect($"{_frontendUrl}/stores/{storeId}/platforms/success");
}
```

---

## ?? Facebook App Setup Steps

### Step 1: Create Facebook App

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Click "My Apps" ? "Create App"
3. Select **"Business"** type
4. Fill in app details:
   - **App Name**: Business Manager
   - **Contact Email**: Your email
5. Click "Create App"

---

### Step 2: Configure Facebook Login

1. In app dashboard, click "Add Product"
2. Select "Facebook Login"
3. Choose "Web" platform
4. Configure settings:
   - **Valid OAuth Redirect URIs**: 
     ```
     https://localhost:5001/api/social-platforms/facebook/callback
     https://yourdomain.com/api/social-platforms/facebook/callback
     ```
   - **Deauthorize Callback URL**: (optional for now)
   - **Data Deletion Request URL**: (optional for now)

---

### Step 3: Request Permissions

Required permissions for posting to Facebook Pages:

1. **pages_show_list** - Get list of user's pages
2. **pages_read_engagement** - Read page insights
3. **pages_manage_posts** - Create/manage posts on pages

**How to Request:**
1. Go to App Dashboard ? App Review ? Permissions and Features
2. Request advanced access for each permission
3. Provide use case description
4. Wait for Facebook approval

**For Development:**
- Add test users in App Dashboard ? Roles ? Test Users
- Test users can use the app without approval

---

### Step 4: Get App Credentials

1. Go to App Dashboard ? Settings ? Basic
2. Copy **App ID** and **App Secret**
3. Update `appsettings.json`:
```json
{
  "Facebook": {
    "AppId": "YOUR_APP_ID_HERE",
    "AppSecret": "YOUR_APP_SECRET_HERE",
    "RedirectUri": "https://localhost:5001/api/social-platforms/facebook/callback"
  }
}
```

---

## ?? Facebook Graph API Endpoints Reference

### 1. Exchange Code for Access Token
```http
GET https://graph.facebook.com/v18.0/oauth/access_token
  ?client_id={app-id}
  &redirect_uri={redirect-uri}
  &client_secret={app-secret}
  &code={authorization-code}

Response:
{
  "access_token": "short-lived-user-token",
  "token_type": "bearer",
  "expires_in": 5183944
}
```

---

### 2. Get User's Pages
```http
GET https://graph.facebook.com/v18.0/me/accounts
  ?access_token={user-access-token}

Response:
{
  "data": [
    {
      "access_token": "page-access-token",
      "category": "Business",
      "name": "My Business Page",
      "id": "123456789",
      "tasks": ["ANALYZE", "ADVERTISE", "MODERATE", "CREATE_CONTENT", "MANAGE"]
    }
  ]
}
```

---

### 3. Get Long-Lived Page Access Token
```http
GET https://graph.facebook.com/v18.0/oauth/access_token
  ?grant_type=fb_exchange_token
  &client_id={app-id}
  &client_secret={app-secret}
  &fb_exchange_token={short-lived-token}

Response:
{
  "access_token": "long-lived-page-token",
  "token_type": "bearer",
  "expires_in": 0  // Never expires for page tokens
}
```

---

### 4. Publish Photo to Page
```http
POST https://graph.facebook.com/v18.0/{page-id}/photos
  ?message={post-caption}
  &url={image-url}
  &access_token={page-access-token}

Response:
{
  "id": "photo_id",
  "post_id": "page_id_post_id"
}
```

---

### 5. Publish Text Post to Page
```http
POST https://graph.facebook.com/v18.0/{page-id}/feed
  ?message={post-caption}
  &access_token={page-access-token}

Response:
{
  "id": "page_id_post_id"
}
```

---

## ?? Testing the Integration

### Local Testing Setup

1. **Start Application**
```bash
cd Presentation
dotnet run
```

2. **Test OAuth Flow**
```bash
# 1. Get authorization URL
curl -X GET "https://localhost:5001/api/social-platforms/facebook/auth-url?storeId={your-store-id}"

# 2. Open URL in browser, authorize app
# 3. Facebook redirects to callback with code
# 4. Callback exchanges code for token and stores platform
```

3. **Create Test Campaign Post**
```bash
curl -X POST "https://localhost:5001/api/campaign-posts" \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "X-Store-ID: {your-store-id}" \
  -H "Content-Type: application/json" \
  -d '{
    "campaignId": "{campaign-guid}",
    "postCaption": "Test post from Business Manager!",
    "postImageUrl": "https://via.placeholder.com/800x600",
    "scheduledAt": "2024-12-20T10:00:00Z"
  }'
```

4. **Monitor Hangfire Dashboard**
```
Navigate to: https://localhost:5001/hangfire
Check: Recurring Jobs ? campaign-scheduler
```

5. **Check Logs**
```bash
# Look for log messages like:
[Information] Found 1 due posts to process
[Information] Successfully published post {PostId} to Facebook with external ID {ExternalId}
```

---

## ?? Common Issues & Troubleshooting

### Issue 1: "Invalid OAuth Redirect URI"
**Cause**: Redirect URI in Facebook app doesn't match `appsettings.json`

**Solution**:
1. Go to Facebook App ? Settings ? Basic ? Add Platform ? Website
2. Add exact redirect URI from `appsettings.json`
3. Must include protocol (https://) and path

---

### Issue 2: "(#200) The user hasn't authorized the application to perform this action"
**Cause**: Missing permissions or page access

**Solution**:
1. Request `pages_manage_posts` permission
2. Ensure user is admin/editor of the page
3. Check page tasks include "CREATE_CONTENT"

---

### Issue 3: "Error validating access token: Session has expired"
**Cause**: Access token expired or invalid

**Solution**:
1. Use long-lived page access tokens (never expire)
2. Implement token refresh logic
3. Check `SocialPlatform.AccessToken` is stored correctly

---

### Issue 4: "(#324) Requires upload file"
**Cause**: Image URL not accessible from Facebook servers

**Solution**:
1. Ensure image URL is publicly accessible
2. Use HTTPS URLs only
3. Test image URL in browser: `curl -I {image-url}`

---

### Issue 5: "Failed to publish photo to Facebook: (OAuthException)"
**Cause**: Various - check error details

**Solution**:
1. Check `CampaignPostPlatforms.ErrorMessage` for specifics
2. Verify page access token in database
3. Test Graph API manually with Graph API Explorer

---

## ?? Database Schema for Social Platforms

```sql
-- SocialPlatforms table
CREATE TABLE SocialPlatforms (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    StoreId UNIQUEIDENTIFIER NOT NULL,
    PlatformName NVARCHAR(50) NOT NULL,  -- 'Facebook', 'Instagram', etc.
    ExternalPageID NVARCHAR(200) NOT NULL,  -- Facebook Page ID
    PageName NVARCHAR(200) NOT NULL,
    AccessToken NVARCHAR(2000) NOT NULL,  -- Long-lived page access token
    IsConnected BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (StoreId) REFERENCES Stores(Id)
);

-- CampaignPostPlatforms table (junction table)
CREATE TABLE CampaignPostPlatforms (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CampaignPostId UNIQUEIDENTIFIER NOT NULL,
    PlatformId UNIQUEIDENTIFIER NOT NULL,
    ExternalPostId NVARCHAR(200),  -- Facebook post ID after publishing
    PublishStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    ScheduledAt DATETIME2 NOT NULL,
    PublishedAt DATETIME2,
    ErrorMessage NVARCHAR(MAX),
    
    FOREIGN KEY (CampaignPostId) REFERENCES CampaignPosts(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlatformId) REFERENCES SocialPlatforms(Id)
);
```

---

## ?? Complete Publishing Flow

```
1. User connects Facebook Page (OAuth flow)
   ?
2. System stores page access token in SocialPlatforms table
   ?
3. User creates campaign post with scheduled time
   ?
4. System creates CampaignPost with PublishStatus.Pending
   ?
5. System creates CampaignPostPlatform records for each connected platform
   ?
6. Hangfire job runs every 5 minutes
   ?
7. CampaignSchedulingService queries due posts
   ?
8. For each due post:
   - Mark as Publishing
   - For each platform:
     ? FacebookPublisher.PublishPostAsync()
     ? Graph API call with page access token
     ? Store external post ID
     ? Mark platform post as Published
   - Mark overall post as Published
   ?
9. User sees post on their Facebook Page!
```

---

## ?? Implementation Checklist

### Phase 1: OAuth Flow
- [ ] Create Facebook App
- [ ] Configure OAuth redirect URIs
- [ ] Request required permissions
- [ ] Update `appsettings.json` with App ID/Secret
- [ ] Implement `GetFacebookAuthUrl()` endpoint
- [ ] Implement `FacebookCallback()` endpoint
- [ ] Implement `HandleFacebookCallbackAsync()` service method
- [ ] Test OAuth flow with test user

### Phase 2: Token Management
- [ ] Implement short-lived to long-lived token exchange
- [ ] Implement page access token acquisition
- [ ] Store tokens in `SocialPlatforms` table
- [ ] Add token refresh logic (optional - page tokens don't expire)

### Phase 3: Publishing
- [ ] Test `FacebookPublisher.cs` with real posts
- [ ] Verify image URL accessibility
- [ ] Test text-only posts
- [ ] Test posts with images
- [ ] Verify error handling

### Phase 4: Monitoring
- [ ] Test Hangfire recurring job
- [ ] Monitor logs for publishing success/failure
- [ ] Verify `PublishStatus` enum values in database
- [ ] Check `ExternalPostId` stored correctly
- [ ] Test failure scenarios

---

## ?? Next Steps

### Immediate Actions:
1. **Create Facebook App** (30 minutes)
2. **Update `appsettings.json`** (5 minutes)
3. **Implement OAuth endpoints** (2-3 hours)
4. **Test with real Facebook page** (1 hour)

### Future Enhancements:
- Instagram integration (similar OAuth flow)
- Post scheduling with time zone support
- Retry failed posts automatically
- Post preview before publishing
- Multi-image posts (carousel)
- Video posts support
- Story posts
- Reel posts

---

## ?? Additional Resources

### Official Documentation
- [Facebook Login Documentation](https://developers.facebook.com/docs/facebook-login)
- [Graph API Reference](https://developers.facebook.com/docs/graph-api)
- [Page Publishing Guide](https://developers.facebook.com/docs/pages/publishing)
- [Access Token Guide](https://developers.facebook.com/docs/facebook-login/guides/access-tokens)

### Testing Tools
- [Graph API Explorer](https://developers.facebook.com/tools/explorer/) - Test API calls
- [Access Token Debugger](https://developers.facebook.com/tools/debug/accesstoken/) - Debug tokens
- [Sharing Debugger](https://developers.facebook.com/tools/debug/) - Preview links

### Code Examples
- [Facebook C# SDK](https://github.com/facebook-csharp-sdk/facebook-csharp-sdk) (optional)
- [Graph API samples](https://developers.facebook.com/docs/graph-api/guides/sample-requests)

---

## ? Questions for AI Assistant

When you're ready to implement Facebook OAuth, ask your AI assistant:

1. **"Help me implement the Facebook OAuth callback handler in `SocialPlatformService.cs`"**
2. **"Write the controller endpoints for Facebook OAuth authorization flow"**
3. **"How do I exchange a short-lived token for a long-lived page access token?"**
4. **"Help me test the `FacebookPublisher.cs` with a real Facebook page"**
5. **"What's the best way to handle expired access tokens in the scheduling service?"**

---

**Good luck with your Facebook integration! ??**

**Generated**: December 2024  
**Version**: 1.0  
**For**: Business Manager Platform
