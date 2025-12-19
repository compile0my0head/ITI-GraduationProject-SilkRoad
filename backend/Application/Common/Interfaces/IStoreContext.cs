namespace Application.Common.Interfaces;

/// <summary>
/// Store Context - Provides access to the currently selected Store ID for the current request
/// Read-only interface for Application and Domain layers
/// StoreId is extracted from HTTP header "X-Store-ID" by middleware
/// </summary>
public interface IStoreContext
{
    /// <summary>
    /// Gets the currently selected Store ID from the HTTP request context
    /// Returns null if no store is selected (non-store-scoped operations)
    /// </summary>
    Guid? StoreId { get; }
    
    /// <summary>
    /// Gets whether a store is currently selected in the request context
    /// </summary>
    bool HasStoreContext { get; }
}
