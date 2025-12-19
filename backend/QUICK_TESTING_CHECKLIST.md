# ?? Quick Testing Checklist

## ? **Pre-Requisites**

- [ ] Facebook Page created
- [ ] Facebook Page ID obtained
- [ ] Page Access Token obtained (long-lived)
- [ ] Store exists in database
- [ ] Campaign exists for the store
- [ ] JWT token for API authentication

---

## ?? **Step-by-Step Testing**

### **Step 1: Connect Facebook Platform**

```sql
-- Insert Facebook platform connection
INSERT INTO SocialPlatforms (
    Id, StoreId, PlatformName, ExternalPageID, PageName, 
    AccessToken, IsConnected, IsDeleted, CreatedAt, UpdatedAt
)
VALUES (
    NEWID(),
    'YOUR_STORE_ID_HERE',          -- ?? Replace with your Store ID
    0,                             -- Facebook
    'YOUR_FACEBOOK_PAGE_ID',       -- ?? Replace with your Page ID
    'My Business Page',
    'YOUR_PAGE_ACCESS_TOKEN',      -- ?? Replace with your Page Token
    1,                             -- IsConnected = true
    0,                             -- IsDeleted = false
    GETUTCDATE(),
    GETUTCDATE()
);
```

**Verify:**
```sql
SELECT * FROM SocialPlatforms 
WHERE StoreId = 'YOUR_STORE_ID' AND IsConnected = 1;
```

Expected: 1 row with Facebook platform

---

### **Step 2: Create Campaign Post**

```bash
POST https://localhost:5001/api/campaign-posts
Authorization: Bearer YOUR_JWT_TOKEN
X-Store-ID: YOUR_STORE_ID
Content-Type: application/json

{
  "campaignId": "YOUR_CAMPAIGN_ID",
  "postCaption": "?? Test post from API! This is a test.",
  "postImageUrl": null,
  "scheduledAt": null
}
```

**Expected Response:**
```json
{
  "id": "new-post-guid",
  "campaignId": "your-campaign-guid",
  "campaignName": "Test Campaign",
  "postCaption": "?? Test post from API!",
  "scheduledAt": "Dec 20, 2024 2:35 PM",
  "createdAt": "Dec 20, 2024 2:35 PM"
}
```

---

### **Step 3: Verify Database Records**

```sql
-- Check CampaignPost
SELECT Id, PostCaption, ScheduledAt, PublishStatus 
FROM CampaignPosts 
WHERE PostCaption LIKE '%Test post from API%';

-- Check CampaignPostPlatforms (CRITICAL!)
SELECT 
    cpp.Id,
    cpp.CampaignPostId,
    sp.PlatformName,
    cpp.PublishStatus,
    cpp.ScheduledAt,
    cpp.ExternalPostId
FROM CampaignPostPlatforms cpp
JOIN SocialPlatforms sp ON cpp.PlatformId = sp.Id
WHERE cpp.CampaignPostId = 'YOUR_POST_ID';
```

**Expected Results:**
- ? 1 row in CampaignPostPlatforms
- ? PlatformName = Facebook (0)
- ? PublishStatus = Pending
- ? ExternalPostId = NULL (not published yet)

---

### **Step 4: Wait for Hangfire Job**

Hangfire runs every minute. Wait ~1 minute.

**Check Hangfire Dashboard:**
```
https://localhost:5001/hangfire/jobs/succeeded
```

Look for `platform-publisher` job.

**Check Application Logs:**
Look for:
```
[Information] Found 1 due platform posts to process
[Information] Publishing text post to Facebook page 123456789
[Information] Successfully published platform post ... with external ID 123456_789
```

---

### **Step 5: Verify Publication**

```sql
-- Check updated status
SELECT 
    cpp.PublishStatus,
    cpp.ExternalPostId,
    cpp.PublishedAt,
    cpp.ErrorMessage
FROM CampaignPostPlatforms cpp
WHERE cpp.CampaignPostId = 'YOUR_POST_ID';
```

**Expected Results:**
- ? PublishStatus = Published
- ? ExternalPostId = "123456789012345_987654321" (Facebook post ID)
- ? PublishedAt = timestamp
- ? ErrorMessage = NULL

**Check Facebook Page:**
```
https://www.facebook.com/YOUR_PAGE_ID
```

? You should see your post!

---

## ?? **Test 2: Post with Image**

```bash
POST https://localhost:5001/api/campaign-posts

{
  "campaignId": "YOUR_CAMPAIGN_ID",
  "postCaption": "?? Test post with image!",
  "postImageUrl": "https://picsum.photos/800/600",
  "scheduledAt": null
}
```

Wait for Hangfire ? Check Facebook ? Should see post with image

---

## ?? **Test 3: Scheduled Post**

```bash
POST https://localhost:5001/api/campaign-posts

{
  "campaignId": "YOUR_CAMPAIGN_ID",
  "postCaption": "? Scheduled for tomorrow!",
  "scheduledAt": "2024-12-26T15:00:00Z"
}
```

**Verify:**
```sql
SELECT ScheduledAt, PublishStatus 
FROM CampaignPostPlatforms 
WHERE CampaignPostId = 'new-post-id';
```

Expected:
- ? ScheduledAt = 2024-12-26 15:00:00
- ? PublishStatus = Pending
- ? Will publish automatically at 15:00 on Dec 26

---

## ? **Error Scenarios**

### **Test 4: Invalid Token**

```sql
-- Temporarily break the token
UPDATE SocialPlatforms 
SET AccessToken = 'INVALID_TOKEN' 
WHERE PlatformName = 0;
```

Create new post ? Wait for Hangfire ? Check error:

```sql
SELECT PublishStatus, ErrorMessage 
FROM CampaignPostPlatforms 
WHERE ...;
```

Expected:
- PublishStatus = Failed
- ErrorMessage = "Facebook API error: Invalid OAuth access token..."

**Fix:**
```sql
UPDATE SocialPlatforms 
SET AccessToken = 'VALID_TOKEN' 
WHERE PlatformName = 0;
```

---

### **Test 5: No Connected Platforms**

```sql
-- Disconnect all platforms
UPDATE SocialPlatforms SET IsConnected = 0;
```

Try to create post:

Expected: `400 Bad Request`
```json
{
  "message": "No connected social platforms found for this store..."
}
```

**Fix:**
```sql
UPDATE SocialPlatforms SET IsConnected = 1 WHERE PlatformName = 0;
```

---

## ? **Success Criteria**

- [ ] CampaignPostPlatform auto-created when CampaignPost is created
- [ ] Hangfire job runs every minute
- [ ] Post published to Facebook successfully
- [ ] ExternalPostId stored in database
- [ ] Post visible on Facebook Page
- [ ] Text-only posts work
- [ ] Text+image posts work
- [ ] Scheduled posts work
- [ ] Error handling works (invalid token returns proper error)
- [ ] Logs show detailed information

---

## ?? **Common Issues**

| Issue | Cause | Solution |
|-------|-------|----------|
| CampaignPostPlatforms empty | No connected platforms | Connect at least one platform |
| Post not publishing | Hangfire not running | Check Program.cs has `app.UseHangfireServer()` |
| Error 190 | Invalid token | Regenerate Page Access Token |
| Error 100 | Invalid Page ID | Verify Page ID is correct |
| Error 200 | Missing permissions | Re-authorize with correct scopes |

---

## ?? **Quick SQL Queries**

```sql
-- See all connected platforms
SELECT * FROM SocialPlatforms WHERE IsConnected = 1;

-- See recent posts
SELECT TOP 10 * FROM CampaignPosts ORDER BY CreatedAt DESC;

-- See platform posts status
SELECT 
    cp.PostCaption,
    sp.PlatformName,
    cpp.PublishStatus,
    cpp.ExternalPostId,
    cpp.ErrorMessage
FROM CampaignPostPlatforms cpp
JOIN CampaignPosts cp ON cpp.CampaignPostId = cp.Id
JOIN SocialPlatforms sp ON cpp.PlatformId = sp.Id
ORDER BY cp.CreatedAt DESC;

-- See failed publications
SELECT * FROM CampaignPostPlatforms 
WHERE PublishStatus = 'Failed' 
ORDER BY ScheduledAt DESC;
```

---

## ?? **Final Verification**

Run this complete test:

1. ? Connect Facebook platform (SQL insert)
2. ? Create campaign post (API call)
3. ? Verify CampaignPostPlatforms created (SQL query)
4. ? Wait for Hangfire job (~1 minute)
5. ? Verify post published (SQL query + Facebook Page)
6. ? Verify ExternalPostId stored (SQL query)

If all 6 steps pass ? **System is working correctly!** ??

---

**Status:** Ready for Testing  
**Last Updated:** December 2024
