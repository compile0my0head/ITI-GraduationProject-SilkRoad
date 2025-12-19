using Application.Common.Interfaces;
using Application.DTOs.Campaigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Campaigns API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[Route("api/campaigns")]
[ApiController]
[Authorize]
public class CampaignsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public CampaignsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    /// <summary>
    /// Get all campaigns
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CampaignDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> GetAllCampaigns()
    {
        try
        {
            var campaigns = await _serviceManager.CampaignService.GetAllAsync();
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving campaigns: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get campaign by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{campaignId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CampaignDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> GetCampaignById(Guid campaignId)
    {
        try
        {
            var campaign = await _serviceManager.CampaignService.GetCampaignByIdAsync(campaignId);
            
            if (campaign == null)
            {
                return NotFound(new { message = $"Campaign with ID '{campaignId}' not found." });
            }
            
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving the campaign: {ex.Message}" });
        }
    }

    /// <summary>
    /// Create a new campaign
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CampaignDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var campaign = await _serviceManager.CampaignService.CreateCampaignAsync(request);
            return CreatedAtAction(nameof(GetCampaignById), new { campaignId = campaign.Id }, campaign);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while creating the campaign: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update an existing campaign
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{campaignId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CampaignDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> UpdateCampaign(Guid campaignId, [FromBody] UpdateCampaignRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var campaign = await _serviceManager.CampaignService.UpdateCampaignAsync(campaignId, request);
            return Ok(campaign);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while updating the campaign: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete a campaign
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{campaignId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> DeleteCampaign(Guid campaignId)
    {
        try
        {
            await _serviceManager.CampaignService.DeleteCampaignAsync(campaignId);
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
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while deleting the campaign: {ex.Message}" });
        }
    }
}