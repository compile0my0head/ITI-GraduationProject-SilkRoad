using Application.Common.Interfaces;
using Application.DTOs.CampaignPosts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Campaign Posts API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[Route("api/campaign-posts")]
[ApiController]
[Authorize]
public class CampaignPostsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public CampaignPostsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    /// <summary>
    /// Get all campaign posts
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CampaignPostDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> GetAllPosts()
    {
        try
        {
            var posts = await _serviceManager.CampaignPostService.GetAllPostsAsync();
            return Ok(posts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving campaign posts: {ex.Message}" });
        }
    }

    /// <summary>
    /// Get campaign post by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{postId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CampaignPostDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> GetPostById(Guid postId)
    {
        try
        {
            var post = await _serviceManager.CampaignPostService.GetPostByIdAsync(postId);
            
            if (post == null)
            {
                return NotFound(new { message = $"Campaign post with ID '{postId}' not found." });
            }
            
            return Ok(post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"An error occurred while retrieving the campaign post: {ex.Message}" });
        }
    }

    /// <summary>
    /// Create a new campaign post
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CampaignPostDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> CreatePost([FromBody] CreateCampaignPostRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var post = await _serviceManager.CampaignPostService.CreatePostAsync(request);

            return CreatedAtAction(
                nameof(GetPostById), 
                new { postId = post.Id }, 
                post);
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
            return StatusCode(500, new { message = $"An error occurred while creating the campaign post: {ex.Message}" });
        }
    }

    /// <summary>
    /// Update an existing campaign post
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{postId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CampaignPostDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> UpdatePost(
        Guid postId, 
        [FromBody] UpdateCampaignPostRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var post = await _serviceManager.CampaignPostService.UpdatePostAsync(postId, request);
            return Ok(post);
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
            return StatusCode(500, new { message = $"An error occurred while updating the campaign post: {ex.Message}" });
        }
    }

    /// <summary>
    /// Delete a campaign post
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{postId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        try
        {
            await _serviceManager.CampaignPostService.DeletePostAsync(postId);
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
            return StatusCode(500, new { message = $"An error occurred while deleting the campaign post: {ex.Message}" });
        }
    }
}
