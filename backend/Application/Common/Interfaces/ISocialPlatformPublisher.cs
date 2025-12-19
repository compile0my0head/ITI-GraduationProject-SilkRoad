using Application.DTOs.Publishing;

namespace Application.Common.Interfaces;

/// <summary>
/// Interface for publishing posts to social media platforms
/// Infrastructure implementations handle platform-specific API calls
/// </summary>
public interface ISocialPlatformPublisher
{
    /// <summary>
    /// Publishes a post to a specific social media platform
    /// </summary>
    /// <param name="request">Publishing request containing post data and platform credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing external post ID or error message</returns>
    Task<PublishPlatformPostResult> PublishAsync(
        PublishPlatformPostRequest request, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the platform name this publisher handles (e.g., "Facebook", "Instagram")
    /// Used by Application layer to route to the correct publisher
    /// </summary>
    string PlatformName { get; }
}
