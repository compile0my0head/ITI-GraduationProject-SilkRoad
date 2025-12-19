using Application.Common.Interfaces;
using Application.DTOs.AutomationTasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// AutomationTasks API Controller
/// STORE-SCOPED endpoints - X-Store-ID required
/// </summary>
[ApiController]
[Route("api/automation-tasks")]
[Produces("application/json")]
[Authorize]
public class AutomationTaskController : ControllerBase
{
    private readonly IAutomationTaskService _automationTaskService;

    public AutomationTaskController(IAutomationTaskService automationTaskService)
    {
        _automationTaskService = automationTaskService;
    }

    /// <summary>
    /// Get all automation tasks
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTasks(CancellationToken cancellationToken)
    {
        var tasks = await _automationTaskService.GetAllAsync(cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Get automation task by ID
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpGet("{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int taskId, CancellationToken cancellationToken)
    {
        var task = await _automationTaskService.GetByIdAsync(taskId, cancellationToken);
        
        if (task == null)
            return NotFound(new { message = $"AutomationTask with ID {taskId} not found" });
        
        return Ok(task);
    }

    /// <summary>
    /// Create a new automation task
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateAutomationTaskRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var task = await _automationTaskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTaskById), new { taskId = task.Id }, task);
    }

    /// <summary>
    /// Update an existing automation task
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpPut("{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateAutomationTaskRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var task = await _automationTaskService.UpdateAsync(taskId, request, cancellationToken);
            return Ok(task);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"AutomationTask with ID {taskId} not found" });
        }
    }

    /// <summary>
    /// Delete an automation task
    /// STORE-SCOPED - X-Store-ID required
    /// </summary>
    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTask(int taskId, CancellationToken cancellationToken)
    {
        try
        {
            await _automationTaskService.DeleteAsync(taskId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"AutomationTask with ID {taskId} not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
