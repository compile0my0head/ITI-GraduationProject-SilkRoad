namespace Domain.Enums;

/// <summary>
/// Status of a post publication to a social platform
/// </summary>
public enum PublishStatus
{
    /// <summary>
    /// Post is waiting to be published
    /// </summary>
    Pending,
    
    /// <summary>
    /// Post is currently being published
    /// </summary>
    Publishing,
    
    /// <summary>
    /// Post was successfully published
    /// </summary>
    Published,
    
    /// <summary>
    /// Post publication failed
    /// </summary>
    Failed
}
