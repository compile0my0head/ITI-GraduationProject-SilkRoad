using Application.DTOs.Products;
using MediatR;

namespace Application.Features.Products.Queries;

/// <summary>
/// Query Request for getting all products
/// Uses MediatR IRequest pattern
/// This represents the "intent" to get all products
/// </summary>
public record GetAllProductsQuery : IRequest<GetProductsResponse>
{
    // Optional: Add filtering parameters
    public int? StoreId { get; init; }
    public bool? InStockOnly { get; init; }
}
