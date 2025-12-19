using Application.Common.Interfaces;
using Application.DTOs.Orders;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Orders API Controller
/// STORE-SCOPED endpoints - X-Store-ID required (except chatbot endpoint)
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ChatbotOrderService _chatbotOrderService;

    public OrderController(IOrderService orderService, ChatbotOrderService chatbotOrderService)
    {
        _orderService = orderService;
        _chatbotOrderService = chatbotOrderService;
    }

    /// <summary>
    /// Get all orders with optional status filter
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    /// <param name="status">Optional: Filter by order status (Pending, Accepted, Shipped, Delivered, Rejected, Cancelled, Refunded)</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatus? status, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(status, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Get order by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
            return NotFound(new { message = $"Order with ID {orderId} not found" });
        
        return Ok(order);
    }

    /// <summary>
    /// Get orders by customer ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("by-customer/{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByCustomerId(Guid customerId, CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetByCustomerIdAsync(customerId, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Create a new order (defaults to Pending status)
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, order);
    }

    /// <summary>
    /// Update an existing order
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{orderId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _orderService.UpdateAsync(orderId, request, cancellationToken);
            return Ok(order);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Order with ID {orderId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an order
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{orderId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteOrder(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.DeleteAsync(orderId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Order with ID {orderId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ==================== ORDER LIFECYCLE ENDPOINTS ====================

    /// <summary>
    /// Accept a pending order
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    /// <remarks>
    /// Changes order status from Pending to Accepted.
    /// Only orders with Pending status can be accepted.
    /// </remarks>
    [HttpPut("{orderId}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptOrder(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.AcceptOrderAsync(orderId, cancellationToken);
            return Ok(order);
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
    /// Reject a pending order
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    /// <remarks>
    /// Changes order status from Pending to Rejected.
    /// Only orders with Pending status can be rejected.
    /// Rejected orders are preserved in the database for record-keeping.
    /// </remarks>
    [HttpPut("{orderId}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectOrder(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.RejectOrderAsync(orderId, cancellationToken);
            return Ok(order);
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
    /// Receive order from chatbot (n8n/Facebook Messenger)
    /// PUBLIC endpoint - NO AUTHENTICATION required
    /// </summary>
    /// <remarks>
    /// This endpoint is called by n8n webhook when a customer places an order via Facebook Messenger.
    /// The order is created with Status = Pending and can be accepted/rejected by admin later.
    /// 
    /// Expected payload from n8n:
    /// {
    ///   "customer": {
    ///     "name": "Customer Name",
    ///     "phone": "0123456789",
    ///     "address": "Customer Address",
    ///     "psid": "facebook_psid_value"
    ///   },
    ///   "items": [
    ///     {
    ///       "productName": "Product Name",
    ///       "quantity": 2
    ///     }
    ///   ],
    ///   "pageId": "facebook_page_id"
    /// }
    /// </remarks>
    [HttpPost("chatbot")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReceiveChatbotOrder(
        [FromBody] ChatbotOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var order = await _chatbotOrderService.ProcessChatbotOrderAsync(request, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetOrderById),
                new { orderId = order.Id },
                new
                {
                    success = true,
                    message = $"Order created successfully with {request.Items.Count} item(s)",
                    orderId = order.Id,
                    status = order.StatusDisplayName,
                    totalPrice = order.TotalPrice,
                    customerName = order.CustomerName
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new 
            { 
                success = false,
                message = ex.Message 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                success = false,
                message = "An error occurred while processing the order",
                error = ex.Message 
            });
        }
    }
}
