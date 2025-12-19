using Application.Common.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Store Context Implementation - Holds the currently selected Store ID for the current HTTP request
/// This is a scoped service (one instance per request)
/// StoreId is set by middleware and read by services/repositories
/// </summary>
public class StoreContext : IStoreContext
{
    private Guid? _storeId;

    /// <summary>
    /// Gets the currently selected Store ID
    /// </summary>
    public Guid? StoreId => _storeId;

    /// <summary>
    /// Gets whether a store is currently selected
    /// </summary>
    public bool HasStoreContext => _storeId.HasValue;

    /// <summary>
    /// Sets the Store ID (called by middleware)
    /// This method should ONLY be called from Infrastructure layer (middleware)
    /// </summary>
    public void SetStoreId(Guid? storeId)
    {
        _storeId = storeId;
    }
}
