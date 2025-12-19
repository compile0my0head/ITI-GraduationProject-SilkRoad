using Application.Common.Interfaces;
using Application.DTOs.Teams;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Teams API Controller
/// Manages team operations and team member management
/// Mixed: GLOBAL + STORE-SCOPED endpoints
/// </summary>
[ApiController]
[Route("api/teams")]
[Produces("application/json")]
[Authorize]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ITeamMemberService _teamMemberService;

    public TeamController(ITeamService teamService, ITeamMemberService teamMemberService)
    {
        _teamService = teamService;
        _teamMemberService = teamMemberService;
    }

    // ==================== GLOBAL TEAM ENDPOINTS ====================

    /// <summary>
    /// Get all teams user has access to (owned stores + team member)
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyTeams(CancellationToken cancellationToken)
    {
        var teams = await _teamService.GetMyTeamsAsync(cancellationToken);
        return Ok(teams);
    }

    /// <summary>
    /// Get team by ID
    /// GLOBAL endpoint - NO X-Store-ID required
    /// </summary>
    [HttpGet("{teamId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamById(Guid teamId, CancellationToken cancellationToken)
    {
        var team = await _teamService.GetByIdAsync(teamId, cancellationToken);
        
        if (team == null)
            return NotFound(new { message = $"Team with ID {teamId} not found" });
        
        return Ok(team);
    }

    // ==================== STORE-SCOPED TEAM ENDPOINTS ====================

    /// <summary>
    /// Get all teams in current store
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTeams(CancellationToken cancellationToken)
    {
        var teams = await _teamService.GetAllAsync(cancellationToken);
        return Ok(teams);
    }

    /// <summary>
    /// Create a new team in current store
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var team = await _teamService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTeamById), new { teamId = team.Id }, team);
    }

    /// <summary>
    /// Update an existing team
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpPut("{teamId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTeam(Guid teamId, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var team = await _teamService.UpdateAsync(teamId, request, cancellationToken);
            return Ok(team);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Team with ID {teamId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a team
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpDelete("{teamId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTeam(Guid teamId, CancellationToken cancellationToken)
    {
        try
        {
            await _teamService.DeleteAsync(teamId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Team with ID {teamId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ==================== TEAM MEMBER ENDPOINTS ====================

    /// <summary>
    /// Get all members of a team
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpGet("{teamId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamMembers(Guid teamId, CancellationToken cancellationToken)
    {
        var members = await _teamMemberService.GetTeamMembersAsync(teamId, cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Add a member to a team
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpPost("{teamId}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTeamMember(Guid teamId, [FromBody] AddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var member = await _teamMemberService.AddMemberAsync(teamId, request.UserId, request.Role, cancellationToken);
            return CreatedAtAction(nameof(GetTeamMembers), new { teamId }, member);
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
    /// Remove a member from a team
    /// STORE-SCOPED endpoint - X-Store-ID required
    /// </summary>
    [HttpDelete("{teamId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveTeamMember(Guid teamId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await _teamMemberService.RemoveMemberAsync(teamId, userId, cancellationToken);
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
