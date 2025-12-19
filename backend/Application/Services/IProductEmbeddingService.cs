using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Service for sending product data to external embedding webhook
/// </summary>
public interface IProductEmbeddingService
{
    /// <summary>
    /// Send product data to embedding webhook asynchronously
    /// Does not block or throw exceptions - failures are logged
    /// </summary>
    Task EmbedProductAsync(Product product, CancellationToken cancellationToken = default);
}
