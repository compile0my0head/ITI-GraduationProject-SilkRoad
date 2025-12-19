using Application.Common.Interfaces;
using Application.DTOs.Publishing;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Publisher for Facebook Pages using Facebook Graph API v24.0
/// Publishes ONLY text-only posts or text+image posts using Page Access Token
/// NO user tokens, NO SDKs, NO Facebook native scheduling
/// </summary>
public class FacebookPublisher : ISocialPlatformPublisher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FacebookPublisher> _logger;
    private const string FACEBOOK_GRAPH_API = "https://graph.facebook.com/v24.0";

    public FacebookPublisher(
        IHttpClientFactory httpClientFactory,
        ILogger<FacebookPublisher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string PlatformName => "Facebook";

    public async Task<PublishPlatformPostResult> PublishAsync(
        PublishPlatformPostRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Validate required fields from SocialPlatforms table
        if (string.IsNullOrEmpty(request.ExternalPageId))
        {
            return PublishPlatformPostResult.Failure("ExternalPageId (Facebook Page ID) is required");
        }

        if (string.IsNullOrEmpty(request.AccessToken))
        {
            return PublishPlatformPostResult.Failure("AccessToken (Page Access Token) is required");
        }

        if (string.IsNullOrEmpty(request.Caption))
        {
            return PublishPlatformPostResult.Failure("Caption (post message) is required");
        }

        var httpClient = _httpClientFactory.CreateClient();
        
        try
        {
            // Example Page ID: 123456789012345 (from SocialPlatforms.ExternalPageID)
            // Example Token: EAATest123... (from SocialPlatforms.AccessToken)
            
            // If there's an image URL, use /photos endpoint
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                return await PublishPhotoAsync(httpClient, request, cancellationToken);
            }
            else
            {
                // Text-only post uses /feed endpoint
                return await PublishTextPostAsync(httpClient, request, cancellationToken);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, 
                "HTTP error publishing to Facebook page {PageId} for platform post {PlatformPostId}", 
                request.ExternalPageId, request.CampaignPostPlatformId);
            return PublishPlatformPostResult.Failure($"HTTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error publishing to Facebook page {PageId} for platform post {PlatformPostId}", 
                request.ExternalPageId, request.CampaignPostPlatformId);
            return PublishPlatformPostResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes a text+image post to Facebook using /{pageId}/photos endpoint
    /// Endpoint: POST https://graph.facebook.com/v24.0/{page-id}/photos
    /// Required params: message, url, access_token
    /// </summary>
    private async Task<PublishPlatformPostResult> PublishPhotoAsync(
        HttpClient httpClient,
        PublishPlatformPostRequest request,
        CancellationToken cancellationToken)
    {
        var postData = new Dictionary<string, string>
        {
            { "message", request.Caption },
            { "url", request.ImageUrl! },
            { "access_token", request.AccessToken }
        };
        
        var photoEndpoint = $"{FACEBOOK_GRAPH_API}/{request.ExternalPageId}/photos";
        
        _logger.LogInformation(
            "Publishing photo to Facebook page {PageId} for platform post {PlatformPostId}",
            request.ExternalPageId, request.CampaignPostPlatformId);

        var response = await httpClient.PostAsync(
            photoEndpoint, 
            new FormUrlEncodedContent(postData), 
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Facebook photo post failed for platform post {PlatformPostId}: {StatusCode} {Error}", 
                request.CampaignPostPlatformId, response.StatusCode, errorContent);
            
            // Try to parse Facebook error for better error message
            var parsedError = TryParseFacebookError(errorContent);
            return PublishPlatformPostResult.Failure($"Facebook API error: {parsedError}");
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var photoResult = JsonSerializer.Deserialize<FacebookPhotoResponse>(jsonContent, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (photoResult?.Id == null)
        {
            _logger.LogError(
                "No post ID returned from Facebook for platform post {PlatformPostId}. Response: {Response}", 
                request.CampaignPostPlatformId, jsonContent);
            return PublishPlatformPostResult.Failure("No post ID returned from Facebook");
        }

        _logger.LogInformation(
            "Successfully published photo to Facebook. Post ID: {PostId}", photoResult.Id);

        return PublishPlatformPostResult.Success(photoResult.Id);
    }

    /// <summary>
    /// Publishes a text-only post to Facebook using /{pageId}/feed endpoint
    /// Endpoint: POST https://graph.facebook.com/v24.0/{page-id}/feed
    /// Required params: message, access_token
    /// </summary>
    private async Task<PublishPlatformPostResult> PublishTextPostAsync(
        HttpClient httpClient,
        PublishPlatformPostRequest request,
        CancellationToken cancellationToken)
    {
        var postData = new Dictionary<string, string>
        {
            { "message", request.Caption },
            { "access_token", request.AccessToken }
        };
        
        var feedEndpoint = $"{FACEBOOK_GRAPH_API}/{request.ExternalPageId}/feed";
        
        _logger.LogInformation(
            "Publishing text post to Facebook page {PageId} for platform post {PlatformPostId}",
            request.ExternalPageId, request.CampaignPostPlatformId);

        var response = await httpClient.PostAsync(
            feedEndpoint, 
            new FormUrlEncodedContent(postData), 
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Facebook feed post failed for platform post {PlatformPostId}: {StatusCode} {Error}", 
                request.CampaignPostPlatformId, response.StatusCode, errorContent);
            
            // Try to parse Facebook error for better error message
            var parsedError = TryParseFacebookError(errorContent);
            return PublishPlatformPostResult.Failure($"Facebook API error: {parsedError}");
        }

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var feedResult = JsonSerializer.Deserialize<FacebookFeedResponse>(jsonContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        if (feedResult?.Id == null)
        {
            _logger.LogError(
                "No post ID returned from Facebook for platform post {PlatformPostId}. Response: {Response}", 
                request.CampaignPostPlatformId, jsonContent);
            return PublishPlatformPostResult.Failure("No post ID returned from Facebook");
        }

        _logger.LogInformation(
            "Successfully published text post to Facebook. Post ID: {PostId}", feedResult.Id);

        return PublishPlatformPostResult.Success(feedResult.Id);
    }

    /// <summary>
    /// Attempts to parse Facebook Graph API error response for better error messages
    /// </summary>
    private string TryParseFacebookError(string errorContent)
    {
        try
        {
            var error = JsonSerializer.Deserialize<FacebookErrorResponse>(errorContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (error?.Error != null)
            {
                return $"{error.Error.Message} (Code: {error.Error.Code}, Type: {error.Error.Type})";
            }
        }
        catch
        {
            // If parsing fails, return raw content
        }

        return errorContent;
    }

    // Response DTOs for Facebook Graph API v24.0
    private class FacebookPhotoResponse
    {
        public string? Id { get; set; }
        public string? PostId { get; set; }
    }

    private class FacebookFeedResponse
    {
        public string? Id { get; set; }
    }

    private class FacebookErrorResponse
    {
        public FacebookError? Error { get; set; }
    }

    private class FacebookError
    {
        public string? Message { get; set; }
        public string? Type { get; set; }
        public int Code { get; set; }
    }
}
