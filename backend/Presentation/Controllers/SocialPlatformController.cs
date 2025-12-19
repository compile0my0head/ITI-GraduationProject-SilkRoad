using Application.Common.Interfaces;
using Application.DTOs.SocialPlatforms;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Social Platforms API Controller
/// Mixed: GLOBAL + STORE-SCOPED endpoints
/// </summary>
[Route("api/social-platforms")]
[ApiController]
[Authorize]
public class SocialPlatformsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public SocialPlatformsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // ==================== GLOBAL ENDPOINT ====================

    /// <summary>
    /// Get all available social platform types (for dropdown selection)
    /// GLOBAL endpoint - NO X-Store-ID required
    /// Returns list of platforms from PlatformName enum
    /// </summary>
    [HttpGet("available-platforms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public IActionResult GetAvailablePlatforms()
    {
        // Get all platform types from enum and format for frontend dropdown
        var platforms = Enum.GetValues<PlatformName>()
            .Select(p => new
            {
                value = (int)p,                    // Enum numeric value (0, 1, 2, 3)
                name = p.ToString(),               // Enum string name (Facebook, Instagram, TikTok, YouTube)
                displayName = p.ToString(),        // Display text for UI
                isOAuthEnabled = p == PlatformName.Facebook // Only Facebook has OAuth implemented for now
            })
            .ToList();

        return Ok(platforms);
    }

    // ==================== STORE-SCOPED ENDPOINTS ====================

    /// <summary>
    /// Get a specific social platform connection by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{connectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialPlatformDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> GetPlatformById(Guid connectionId)
    {
        try
        {
            var platform = await _serviceManager.SocialPlatformService.GetPlatformByIdAsync(connectionId);

            if (platform == null)
            {
                return NotFound(new { message = $"Social platform connection with ID '{connectionId}' not found." });
            }

            return Ok(platform);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving the platform: {ex.Message}" });
        }
    }

    /// <summary>
    /// Manually create a social platform connection (Phase 1 - Testing Only)
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SocialPlatformDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> CreatePlatform([FromBody] CreateSocialPlatformRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var platform = await _serviceManager.SocialPlatformService.CreatePlatformAsync(request);

            return CreatedAtAction(
                nameof(GetPlatformById),
                new { connectionId = platform.Id },
                platform);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Connect a Facebook account via OAuth (Production Ready)
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost("facebook/connect")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialPlatformDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> ConnectFacebook([FromBody] ConnectFacebookRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var platform = await _serviceManager.SocialPlatformService.ConnectFacebookAsync(request);
            return Ok(platform);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { message = $"Failed to connect to Facebook: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while connecting Facebook: {ex.Message}" });
        }
    }

    /// <summary>
    /// Connect an Instagram account via OAuth (Future Implementation)
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost("instagram/connect")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialPlatformDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> ConnectInstagram([FromBody] ConnectInstagramRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var platform = await _serviceManager.SocialPlatformService.ConnectInstagramAsync(request);
            return Ok(platform);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(501, new { message = "Instagram integration is not yet implemented. Please check back later." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while connecting Instagram: {ex.Message}" });
        }
    }

    /// <summary>
    /// Disconnect a social platform (soft delete - keeps data but marks as disconnected)
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{connectionId:guid}/disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SocialPlatformDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> DisconnectPlatform(Guid connectionId)
    {
        try
        {
            var platform = await _serviceManager.SocialPlatformService.DisconnectPlatformAsync(connectionId);
            return Ok(platform);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while disconnecting the platform: {ex.Message}" });
        }
    }

    /// <summary>
    /// Permanently delete a social platform connection
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{connectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> DeletePlatform(Guid connectionId)
    {
        try
        {
            await _serviceManager.SocialPlatformService.DeletePlatformAsync(connectionId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while deleting the platform: {ex.Message}" });
        }
    }
}
