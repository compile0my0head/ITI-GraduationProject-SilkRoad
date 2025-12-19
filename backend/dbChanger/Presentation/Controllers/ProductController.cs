using Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Products API Controller
/// This is the entry point for product-related HTTP requests
/// Uses MediatR to send queries/commands to handlers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    // MediatR is injected - it will route our requests to the correct handler
    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <param name="storeId">Optional: Filter by store ID</param>
    /// <param name="inStockOnly">Optional: Only return in-stock products</param>
    /// <returns>List of products</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int? storeId,
        [FromQuery] bool? inStockOnly,
        CancellationToken cancellationToken)
    {
        // Create the query with parameters
        var query = new GetAllProductsQuery
        {
            StoreId = storeId,
            InStockOnly = inStockOnly
        };

        // Send query to MediatR - it will find and execute GetAllProductsHandler
        var response = await _mediator.Send(query, cancellationToken);

        // Return HTTP 200 OK with the response data
        return Ok(response);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Single product</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id)
    {
        // TODO: Create GetProductByIdQuery and Handler
        // For now, return a placeholder
        return Ok(new { message = $"Get product with ID {id}" });
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct()
    {
        // TODO: Create CreateProductCommand and Handler
        return Ok(new { message = "Create product endpoint - to be implemented" });
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id)
    {
        // TODO: Create UpdateProductCommand and Handler
        return Ok(new { message = $"Update product {id} endpoint - to be implemented" });
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        // TODO: Create DeleteProductCommand and Handler
        return Ok(new { message = $"Delete product {id} endpoint - to be implemented" });
    }
}
