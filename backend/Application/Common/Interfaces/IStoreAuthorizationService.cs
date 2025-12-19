namespace Application.Common.Interfaces;

/// <summary>
/// Store Authorization Service - Validates if the current user has access to a specific store
/// User has access if they are either:
/// 1. The store owner (Store.OwnerUserId)
/// 2. A team member of any team in that store (TeamMember table)
/// </summary>
public interface IStoreAuthorizationService
{
    /// <summary>
    /// Checks if the current authenticated user belongs to the specified store
    /// Returns true if user is owner or team member
    /// </summary>
    Task<bool> UserBelongsToStoreAsync(Guid storeId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates store access and throws UnauthorizedAccessException if user doesn't belong
    /// </summary>
    Task ValidateStoreAccessAsync(Guid storeId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all store IDs that the user has access to (owned stores + team member stores)
    /// </summary>
    Task<List<Guid>> GetUserStoreIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}
