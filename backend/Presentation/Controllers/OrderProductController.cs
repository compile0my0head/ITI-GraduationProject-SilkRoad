using Application.Common.Interfaces;
using Application.DTOs.OrderProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// OrderProducts API Controller
/// Manages products within orders
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class OrderProductController : ControllerBase
{
    private readonly IOrderProductService _orderProductService;

    public OrderProductController(IOrderProductService orderProductService)
    {
        _orderProductService = orderProductService;
    }

    /// <summary>
    /// Get all products in an order
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderProducts(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var orderProducts = await _orderProductService.GetByOrderIdAsync(orderId, cancellationToken);
            return Ok(orderProducts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add a product to an order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddProductToOrder([FromBody] AddProductToOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var orderProduct = await _orderProductService.AddProductToOrderAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetOrderProducts), new { orderId = request.OrderId }, orderProduct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update the quantity of a product in an order
    /// </summary>
    [HttpPut("{orderId}/product/{productId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductQuantity(Guid orderId, Guid productId, [FromBody] UpdateOrderProductQuantityRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var orderProduct = await _orderProductService.UpdateQuantityAsync(orderId, productId, request.Quantity, cancellationToken);
            return Ok(orderProduct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a product from an order
    /// </summary>
    [HttpDelete("{orderId}/product/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveProductFromOrder(Guid orderId, Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            await _orderProductService.RemoveProductFromOrderAsync(orderId, productId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
