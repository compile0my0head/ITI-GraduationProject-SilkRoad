using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Stores;
using Microsoft.AspNetCore.Authorization;

namespace Presentation.Controllers;

[Route("api/stores")]
[ApiController]
public class StoreController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public StoreController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    /// <summary>
    /// Get user's accessible stores (owned + team member)
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(List<StoreDto>))]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<List<StoreDto>>> GetMyStores()
    {
        var stores = await _serviceManager.StoreService.GetMyStoresAsync();
        return Ok(stores);
    }

    /// <summary>
    /// Get store by ID
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("{storeId}")]
    [ProducesResponseType(200, Type = typeof(StoreDto))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<StoreDto>> GetStoreById(Guid storeId)
    {
        var store = await _serviceManager.StoreService.GetStoreByIdAsync(storeId);
        
        if (store == null)
        {
            throw new KeyNotFoundException($"Store with ID {storeId} not found.");
        }
        
        return Ok(store);
    }

    /// <summary>
    /// Create a new store
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(201, Type = typeof(StoreDto))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<StoreDto>> CreateStore([FromBody] CreateStoreRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var store = await _serviceManager.StoreService.CreateStoreAsync(request);
        return CreatedAtAction(nameof(GetStoreById), new { storeId = store.Id }, store);
    }

    /// <summary>
    /// Update an existing store
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpPut("{storeId}")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(StoreDto))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<StoreDto>> UpdateStore(Guid storeId, [FromBody] UpdateStoreRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var store = await _serviceManager.StoreService.UpdateStoreAsync(storeId, request);
        return Ok(store);
    }

    /// <summary>
    /// Delete a store
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpDelete("{storeId}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> DeleteStore(Guid storeId)
    {
        try
        {
            await _serviceManager.StoreService.DeleteStoreAsync(storeId);
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
