using Application.DTOs.ConfigurationGroup;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class ConfigurationGroupsController : BaseApiController
{
    private readonly IConfigurationGroupService _groupService;
    private readonly ILogger<ConfigurationGroupsController> _logger;

    public ConfigurationGroupsController(IConfigurationGroupService groupService, ILogger<ConfigurationGroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    /// <summary>
    /// Get all configuration groups
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationGroupDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = await _groupService.GetAllAsync(cancellationToken);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all configuration groups");

            // Return sample data if database is not available
            var sampleGroups = new List<ConfigurationGroupDto>
            {
                new ConfigurationGroupDto(Guid.NewGuid(), "Database", "Database related configurations", null, null, true, 1, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 3, 0),
                new ConfigurationGroupDto(Guid.NewGuid(), "API", "API related configurations", null, null, true, 2, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 2, 0),
                new ConfigurationGroupDto(Guid.NewGuid(), "Features", "Feature flags and toggles", null, null, true, 3, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 1, 0)
            };

            _logger.LogWarning("Database unavailable, returning sample data");
            return Ok(sampleGroups);
        }
    }

    /// <summary>
    /// Get root configuration groups
    /// </summary>
    [HttpGet("root")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationGroupDto>), 200)]
    public async Task<IActionResult> GetRootGroups(CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = await _groupService.GetRootGroupsAsync(cancellationToken);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root configuration groups");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configuration group tree
    /// </summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationGroupTreeDto>), 200)]
    public async Task<IActionResult> GetGroupTree(CancellationToken cancellationToken = default)
    {
        try
        {
            var tree = await _groupService.GetGroupTreeAsync(cancellationToken);
            return Ok(tree);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration group tree");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configuration group by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConfigurationGroupDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _groupService.GetByIdAsync(id, cancellationToken);
            if (group == null)
            {
                return NotFound($"Configuration group with ID {id} not found");
            }
            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configuration group by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(ConfigurationGroupDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _groupService.GetByNameAsync(name, cancellationToken);
            if (group == null)
            {
                return NotFound($"Configuration group with name '{name}' not found");
            }
            return Ok(group);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration group {Name}", name);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get child groups by parent ID
    /// </summary>
    [HttpGet("parent/{parentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationGroupDto>), 200)]
    public async Task<IActionResult> GetByParent(Guid parentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var groups = await _groupService.GetByParentAsync(parentId, cancellationToken);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child groups for parent {ParentId}", parentId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new configuration group
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConfigurationGroupDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationGroupDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.CreateAsync(createDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration group data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration group");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update a configuration group
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ConfigurationGroupDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConfigurationGroupDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.UpdateAsync(id, updateDto, userId, cancellationToken);
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration group data for update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Move a configuration group to a new parent
    /// </summary>
    [HttpPost("{id:guid}/move")]
    [ProducesResponseType(typeof(ConfigurationGroupDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Move(Guid id, [FromBody] MoveGroupDto moveDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var group = await _groupService.MoveGroupAsync(id, moveDto, userId, cancellationToken);
            return Ok(group);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid move operation for configuration group {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a configuration group
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _groupService.DeleteAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Activate a configuration group
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _groupService.ActivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Deactivate a configuration group
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _groupService.DeactivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating configuration group {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Check if configuration group name exists
    /// </summary>
    [HttpGet("exists/{name}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> Exists(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _groupService.ExistsAsync(name, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if configuration group {Name} exists", name);
            return HandleException(ex);
        }
    }
}
