using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of product embedding service
/// Sends product data to external n8n webhook for vector embedding
/// </summary>
public class ProductEmbeddingService : IProductEmbeddingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductEmbeddingService> _logger;
    private const string EMBEDDING_WEBHOOK_URL = "https://mahmoud-talaat.app.n8n.cloud/webhook-test/embed-products";

    public ProductEmbeddingService(
        IHttpClientFactory httpClientFactory,
        ILogger<ProductEmbeddingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task EmbedProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                productId = product.Id,
                storeId = product.StoreId,
                name = product.ProductName,
                description = product.ProductDescription,
                price = product.ProductPrice,
                inStock = product.InStock
            };

            var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10); // Don't wait too long

            _logger.LogInformation(
                "Sending product {ProductId} to embedding webhook", 
                product.Id);

            var response = await httpClient.PostAsync(EMBEDDING_WEBHOOK_URL, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully embedded product {ProductId}", 
                    product.Id);
            }
            else
            {
                _logger.LogWarning(
                    "Embedding webhook returned status {StatusCode} for product {ProductId}",
                    response.StatusCode,
                    product.Id);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Embedding request cancelled for product {ProductId}", 
                product.Id);
        }
        catch (Exception ex)
        {
            // Log but don't throw - embedding should not break product creation
            _logger.LogError(
                ex,
                "Failed to send product {ProductId} to embedding webhook: {Error}",
                product.Id,
                ex.Message);
        }
    }
}
