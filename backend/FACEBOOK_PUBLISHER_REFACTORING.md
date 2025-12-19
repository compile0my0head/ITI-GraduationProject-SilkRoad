# Facebook Publisher Refactoring - DTO-Based Architecture

## Overview
This refactoring removes entity dependencies from the Infrastructure layer and introduces DTO-based communication between Application and Infrastructure layers, following Clean Architecture principles.

## Changes Made

### 1. New DTOs Created

#### `Application/DTOs/Publishing/PublishPlatformPostRequest.cs`
- **Purpose**: Transfer publishing data from Application to Infrastructure
- **Properties**:
  - `Caption` (string, required): Post caption/message content
  - `ImageUrl` (string?, optional): Optional image URL
  - `AccessToken` (string, required): Platform access token
  - `ExternalPageId` (string, required): Platform page/account identifier
  - `CampaignPostPlatformId` (Guid): For logging/tracking

#### `Application/DTOs/Publishing/PublishPlatformPostResult.cs`
- **Purpose**: Return publishing result from Infrastructure to Application
- **Properties**:
  - `IsSuccess` (bool): Operation success indicator
  - `ExternalPostId` (string?): Platform-returned post ID
  - `ErrorMessage` (string?): Error details if failed
- **Helper Methods**:
  - `Success(string externalPostId)`: Creates success result
  - `Failure(string errorMessage)`: Creates failure result

### 2. Interface Updates

#### `Application/Common/Interfaces/ISocialPlatformPublisher.cs`
**Before**:
```csharp
Task<string> PublishPostAsync(CampaignPost campaignPost, SocialPlatform platform, CancellationToken cancellationToken);
bool SupportsPlatform(string platformName);
```

**After**:
```csharp
Task<PublishPlatformPostResult> PublishAsync(PublishPlatformPostRequest request, CancellationToken cancellationToken);
string PlatformName { get; }
```

**Key Changes**:
- Removed entity parameters (`CampaignPost`, `SocialPlatform`)
- Introduced DTO-based communication
- Replaced `SupportsPlatform` method with `PlatformName` property
- Returns structured result instead of throwing exceptions

### 3. FacebookPublisher Refactoring

#### `Infrastructure/Services/FacebookPublisher.cs`

**Removed Dependencies**:
- ? No more `Domain.Entities` references
- ? No more `IConfiguration` dependency (not needed)
- ? No platform detection logic
- ? No entity usage

**New Structure**:
```csharp
public class FacebookPublisher : ISocialPlatformPublisher
{
    public string PlatformName => "Facebook";
    
    public async Task<PublishPlatformPostResult> PublishAsync(
        PublishPlatformPostRequest request, 
        CancellationToken cancellationToken)
    {
        // Build Facebook API request from DTO
        // Call Facebook Graph API
        // Return structured result (never throw)
    }
}
```

**Error Handling**:
- No more thrown exceptions
- Returns `PublishPlatformPostResult.Failure()` for all errors
- Graceful HTTP error handling
- Comprehensive logging

### 4. Application Layer Updates

#### `Application/Services/Publishing/PlatformPublishingService.cs`

**Updated Routing Logic**:
```csharp
// OLD: p.SupportsPlatform(platformPost.Platform.PlatformName.ToString())
// NEW: p.PlatformName.Equals(platformName, StringComparison.OrdinalIgnoreCase)
```

**DTO Construction**:
```csharp
var publishRequest = new PublishPlatformPostRequest
{
    Caption = platformPost.CampaignPost.PostCaption,
    ImageUrl = platformPost.CampaignPost.PostImageUrl,
    AccessToken = platformPost.Platform.AccessToken,
    ExternalPageId = platformPost.Platform.ExternalPageID,
    CampaignPostPlatformId = platformPost.Id
};

var result = await publisher.PublishAsync(publishRequest, cancellationToken);
```

**Result Handling**:
```csharp
if (result.IsSuccess)
{
    // Update as Published
    platformPost.ExternalPostId = result.ExternalPostId;
    platformPost.PublishStatus = PublishStatus.Published.ToString();
}
else
{
    // Update as Failed
    await MarkAsFailed(platformPost, result.ErrorMessage ?? "Unknown error", cancellationToken);
}
```

#### `Application/Services/CampaignSchedulingService.cs` (Legacy)

**Status**: Updated to use new interface, marked as LEGACY
**Note**: For new implementations, use `PlatformPublishingService`
**Changes**: Same DTO-based approach as PlatformPublishingService

### 5. Dependency Injection

**No Changes Required**: 
- DI registration already in place
- Both services registered and working
- `FacebookPublisher` registered as `ISocialPlatformPublisher`

## Architecture Benefits

### ? Clean Separation of Concerns
- **Application Layer**: Orchestrates business logic, uses repositories and DTOs
- **Infrastructure Layer**: Handles external API calls, no business logic
- **Clear Boundary**: DTOs define the contract between layers

### ? No Entity Leakage
- Infrastructure layer has ZERO entity dependencies
- Domain remains isolated
- DTOs provide explicit contracts

### ? Testability
- Publishers can be easily mocked with DTO contracts
- No need to mock complex entity graphs
- Clear input/output contracts

### ? Extensibility
- Adding new platform publishers is straightforward
- Implement `ISocialPlatformPublisher`
- Set `PlatformName` property
- Handle `PublishPlatformPostRequest`
- Return `PublishPlatformPostResult`

### ? Error Handling
- No exceptions thrown from Infrastructure
- Structured error results
- Application layer decides how to handle failures
- Comprehensive logging at both layers

## Facebook Graph API Implementation

### Photo Posts (with image)
```
Endpoint: /{page-id}/photos
Method: POST
Body: 
  - message: {caption}
  - url: {imageUrl}
  - access_token: {token}
```

### Text Posts (no image)
```
Endpoint: /{page-id}/feed
Method: POST
Body:
  - message: {caption}
  - access_token: {token}
```

## Migration Path for Other Publishers

When creating new publishers (Instagram, Twitter, LinkedIn, etc.):

1. **Create Publisher Class**
   ```csharp
   public class InstagramPublisher : ISocialPlatformPublisher
   {
       public string PlatformName => "Instagram";
       
       public async Task<PublishPlatformPostResult> PublishAsync(
           PublishPlatformPostRequest request,
           CancellationToken cancellationToken)
       {
           // Platform-specific implementation
       }
   }
   ```

2. **Register in DI**
   ```csharp
   services.AddScoped<ISocialPlatformPublisher, InstagramPublisher>();
   ```

3. **No Application Layer Changes Needed**
   - Routing happens automatically via `PlatformName`
   - Application layer is platform-agnostic

## Testing Strategy

### Unit Testing Publishers
```csharp
[Fact]
public async Task PublishAsync_WithImage_ReturnsSuccess()
{
    // Arrange
    var request = new PublishPlatformPostRequest
    {
        Caption = "Test post",
        ImageUrl = "https://example.com/image.jpg",
        AccessToken = "token",
        ExternalPageId = "12345"
    };
    
    // Act
    var result = await _publisher.PublishAsync(request, CancellationToken.None);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.ExternalPostId);
}
```

### Integration Testing
- Mock `IHttpClientFactory` to return test responses
- Verify DTO construction in Application layer
- Verify result handling

## Status Flow Diagram

```
CampaignPostPlatform
    ?
[Pending]
    ?
PlatformPublishingService.ProcessDuePlatformPostsAsync()
    ?
[Publishing] ? Status updated before calling publisher
    ?
Application Layer builds PublishPlatformPostRequest DTO
    ?
Routes to correct publisher via PlatformName
    ?
FacebookPublisher.PublishAsync(request)
    ?
Calls Facebook Graph API
    ?
Returns PublishPlatformPostResult
    ?
Application Layer handles result
    ?
[Published] ? Success (ExternalPostId stored)
    OR
[Failed] ? Failure (ErrorMessage stored)
```

## Backwards Compatibility

- ? Existing `CampaignSchedulingService` updated to use new interface
- ? No database schema changes required
- ? No breaking changes to API endpoints
- ? `CampaignPostPlatform` entity unchanged
- ? Hangfire job continues to work

## Code Quality

- ? No magic strings (except Graph API URL constant)
- ? Clear method naming
- ? Comprehensive XML documentation
- ? Consistent error handling
- ? Follows project patterns (record DTOs, folder structure)

## Build Status

? **All files compile successfully**
? **No breaking changes**
? **All dependencies wired correctly**

## Next Steps (Future Enhancements)

1. **Add Instagram Publisher**
   - Implement `ISocialPlatformPublisher`
   - Handle Instagram Graph API
   - Register in DI

2. **Add Twitter/X Publisher**
   - Implement `ISocialPlatformPublisher`
   - Handle Twitter API v2
   - Register in DI

3. **Add Retry Logic** (Optional)
   - Can be added in Application layer
   - Use Polly for transient failure handling
   - Respect platform rate limits

4. **Add Telemetry** (Optional)
   - Application Insights integration
   - Track publish success/failure rates
   - Monitor API response times

5. **Migrate Away from Legacy CampaignSchedulingService**
   - Gradually transition to `PlatformPublishingService`
   - Remove `CampaignSchedulingService` when no longer needed
   - Update Hangfire job to call new service

---

**Refactoring Completed**: ?  
**Build Status**: ? Successful  
**Architecture Compliance**: ? Clean Architecture principles followed  
**Entity Dependency**: ? Removed from Infrastructure layer  
