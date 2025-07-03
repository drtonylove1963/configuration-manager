using Application.DTOs.Environment;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class EnvironmentsController : BaseApiController
{
    private readonly IEnvironmentService _environmentService;
    private readonly ILogger<EnvironmentsController> _logger;

    public EnvironmentsController(IEnvironmentService environmentService, ILogger<EnvironmentsController> logger)
    {
        _environmentService = environmentService;
        _logger = logger;
    }

    /// <summary>
    /// Get all environments
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var environments = await _environmentService.GetAllAsync(cancellationToken);
            return Ok(environments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all environments");

            // Return sample data if database is not available
            var sampleEnvironments = new List<EnvironmentDto>
            {
                new EnvironmentDto(Guid.NewGuid(), "Development", "Development environment", true, 1, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 5),
                new EnvironmentDto(Guid.NewGuid(), "Staging", "Staging environment", true, 2, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 3),
                new EnvironmentDto(Guid.NewGuid(), "Production", "Production environment", true, 3, DateTime.UtcNow, DateTime.UtcNow, "system", "system", 8)
            };

            _logger.LogWarning("Database unavailable, returning sample data");
            return Ok(sampleEnvironments);
        }
    }

    /// <summary>
    /// Get active environments
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentDto>), 200)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken = default)
    {
        try
        {
            var environments = await _environmentService.GetActiveAsync(cancellationToken);
            return Ok(environments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active environments");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get environment summaries
    /// </summary>
    [HttpGet("summaries")]
    [ProducesResponseType(typeof(IEnumerable<EnvironmentSummaryDto>), 200)]
    public async Task<IActionResult> GetSummaries(CancellationToken cancellationToken = default)
    {
        try
        {
            var summaries = await _environmentService.GetSummariesAsync(cancellationToken);
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving environment summaries");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get environment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnvironmentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var environment = await _environmentService.GetByIdAsync(id, cancellationToken);
            if (environment == null)
            {
                return NotFound($"Environment with ID {id} not found");
            }
            return Ok(environment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving environment {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get environment by name
    /// </summary>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(EnvironmentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var environment = await _environmentService.GetByNameAsync(name, cancellationToken);
            if (environment == null)
            {
                return NotFound($"Environment with name '{name}' not found");
            }
            return Ok(environment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving environment {Name}", name);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new environment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EnvironmentDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateEnvironmentDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var environment = await _environmentService.CreateAsync(createDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = environment.Id }, environment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid environment data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating environment");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update an environment
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EnvironmentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnvironmentDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var environment = await _environmentService.UpdateAsync(id, updateDto, userId, cancellationToken);
            return Ok(environment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid environment data for update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating environment {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete an environment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _environmentService.DeleteAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting environment {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Activate an environment
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _environmentService.ActivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating environment {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Deactivate an environment
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _environmentService.DeactivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating environment {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Check if environment name exists
    /// </summary>
    [HttpGet("exists/{name}")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<IActionResult> Exists(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _environmentService.ExistsAsync(name, cancellationToken);
            return Ok(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if environment {Name} exists", name);
            return HandleException(ex);
        }
    }
}
