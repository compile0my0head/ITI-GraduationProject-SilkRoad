using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Authentication Controller
/// GLOBAL endpoints - NO X-Store-ID required
/// </summary>
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authenticationService;

    public AuthController(IAuthService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// User login - GLOBAL (NO StoreId)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(UserResponseDto))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserResponseDto>> Login([FromBody] LoginRequestDto loginDto)
    {
        var response = await _authenticationService.LoginAsync(loginDto);
        return Ok(response);
    }

    /// <summary>
    /// User registration - GLOBAL (NO StoreId)
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(200, Type = typeof(UserResponseDto))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<ActionResult<UserResponseDto>> Register([FromBody] RegisterRequestDto registerDto)
    {
        var response = await _authenticationService.RegisterAsync(registerDto);
        return Ok(response);
    }

    /// <summary>
    /// User logout - GLOBAL (NO StoreId)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesErrorResponseType(typeof(void))]
    public async Task<IActionResult> Logout()
    {
        // Implement logout logic (e.g., invalidate refresh token)
        // For now, return success as JWT tokens are stateless
        return Ok(new { message = "Logged out successfully" });
    }
}