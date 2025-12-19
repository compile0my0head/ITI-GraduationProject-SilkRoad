using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Products;

namespace Presentation.Controllers;

/// <summary>
/// Products API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products with optional filtering
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] bool? inStockOnly,
        CancellationToken cancellationToken)
    {
        var response = await _productService.GetAllAsync(inStockOnly, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get product by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        
        if (product == null)
            return NotFound(new { message = $"Product with ID {productId} not found" });
        
        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { productId = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(
        Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var product = await _productService.UpdateAsync(productId, request, cancellationToken);
            return Ok(product);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Product with ID {productId} not found" });
        }
    }

    /// <summary>
    /// Delete a product
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(productId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Product with ID {productId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
