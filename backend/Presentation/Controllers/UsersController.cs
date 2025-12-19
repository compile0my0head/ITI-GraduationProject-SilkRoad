using Application.Common.Interfaces;
using Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Users API Controller
/// GLOBAL endpoints - NO X-Store-ID required
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get current authenticated user profile
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        
        if (userId == null)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var user = await _userService.GetByIdAsync(userId.Value, cancellationToken);
        
        if (user == null)
            return NotFound(new { message = "User not found" });
        
        return Ok(user);
    }

    /// <summary>
    /// Get user by ID
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        
        if (user == null)
            return NotFound(new { message = $"User with ID {id} not found" });
        
        return Ok(user);
    }

    /// <summary>
    /// Get user by email
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByEmailAsync(email, cancellationToken);
        
        if (user == null)
            return NotFound(new { message = $"User with email {email} not found" });
        
        return Ok(user);
    }

    /// <summary>
    /// Update current user profile
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _userService.UpdateAsync(id, request, cancellationToken);
            return Ok(user);
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
    /// Delete a user
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _userService.DeleteAsync(id, cancellationToken);
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
