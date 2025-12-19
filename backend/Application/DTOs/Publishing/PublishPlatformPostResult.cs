namespace Application.DTOs.Publishing;

/// <summary>
/// Result DTO returned after attempting to publish a post to a social media platform
/// </summary>
public record PublishPlatformPostResult
{
    /// <summary>
    /// Indicates whether the publish operation succeeded
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// External post ID returned by the platform (e.g., Facebook post ID)
    /// Populated only when IsSuccess is true
    /// </summary>
    public string? ExternalPostId { get; init; }

    /// <summary>
    /// Error message if the publish operation failed
    /// Populated only when IsSuccess is false
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static PublishPlatformPostResult Success(string externalPostId) =>
        new() { IsSuccess = true, ExternalPostId = externalPostId };

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static PublishPlatformPostResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
