using Application.Common.Interfaces;
using Application.DTOs.Products;
using Application.Features.Products.Queries;
using MediatR;

namespace Application.Features.Products.Handlers;

/// <summary>
/// Handler for GetAllProductsQuery
/// This is where the business logic for retrieving products lives
/// MediatR will automatically wire this up to handle GetAllProductsQuery requests
/// </summary>
public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, GetProductsResponse>
{
    private readonly IProductRepository _productRepository;

    // Constructor Injection - Repository is injected by DI container
    public GetAllProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// Handle the query request
    /// </summary>
    /// <param name="request">The query with optional filters</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Response containing list of products</returns>
    public async Task<GetProductsResponse> Handle(
        GetAllProductsQuery request, 
        CancellationToken cancellationToken)
    {
        // Step 1: Get products from repository
        var products = request.StoreId.HasValue
            ? await _productRepository.GetByStoreIdAsync(request.StoreId.Value, cancellationToken)
            : await _productRepository.GetAllAsync(cancellationToken);

        // Step 2: Apply optional filters
        if (request.InStockOnly == true)
        {
            products = products.Where(p => p.InStock).ToList();
        }

        // Step 3: Map domain entities to DTOs
        // In a real app, use AutoMapper for this
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price, // FIX: Use p.Price directly, since it's a decimal
            Currency = "",   // FIX: Set Currency to empty string or appropriate value
            InStock = p.InStock,
            StoreId = p.StoreId,
            CreatedAt = p.CreatedAt
        }).ToList();

        // Step 4: Return response
        return new GetProductsResponse
        {
            Products = productDtos,
            TotalCount = productDtos.Count,
            Message = $"Successfully retrieved {productDtos.Count} product(s)"
        };
    }
}

