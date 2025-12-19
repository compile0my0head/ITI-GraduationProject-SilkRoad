using Application.Common.Interfaces;
using Application.DTOs.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Customers API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[ApiController]
[Route("api/customers")]
[Produces("application/json")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get all customers
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCustomers(CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(cancellationToken);
        return Ok(customers);
    }

    /// <summary>
    /// Get customer by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(Guid customerId, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(customerId, cancellationToken);
        
        if (customer == null)
            return NotFound(new { message = $"Customer with ID {customerId} not found" });
        
        return Ok(customer);
    }

    /// <summary>
    /// Create a new customer
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var customer = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerById), new { customerId = customer.Id }, customer);
    }

    /// <summary>
    /// Update an existing customer
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCustomer(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var customer = await _customerService.UpdateAsync(customerId, request, cancellationToken);
            return Ok(customer);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Customer with ID {customerId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a customer
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{customerId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            await _customerService.DeleteAsync(customerId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Customer with ID {customerId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
