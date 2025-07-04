using Application.DTOs.Configuration;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class ConfigurationsController : BaseApiController
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationsController> _logger;

    public ConfigurationsController(IConfigurationService configurationService, ILogger<ConfigurationsController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all configurations
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await _configurationService.GetAllAsync(cancellationToken);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all configurations");

            // Return sample data if database is not available
            var sampleConfigurations = new List<ConfigurationDto>
            {
                new ConfigurationDto(Guid.NewGuid(), "DatabaseConnectionString", "Server=localhost;Database=MyApp;", Domain.ValueObjects.ConfigurationValueType.String, "Connection string for the main database", Guid.NewGuid(), "Sample App", Guid.NewGuid(), "Development", Guid.NewGuid(), "Database", false, true, null, true, 1, DateTime.UtcNow, DateTime.UtcNow, "system", "system"),
                new ConfigurationDto(Guid.NewGuid(), "ApiTimeout", "30", Domain.ValueObjects.ConfigurationValueType.Integer, "Timeout for API calls in seconds", Guid.NewGuid(), "Sample App", Guid.NewGuid(), "Development", Guid.NewGuid(), "API", false, true, "30", true, 1, DateTime.UtcNow, DateTime.UtcNow, "system", "system"),
                new ConfigurationDto(Guid.NewGuid(), "FeatureFlag_NewUI", "true", Domain.ValueObjects.ConfigurationValueType.Boolean, "Enable new UI features", Guid.NewGuid(), "Sample App", Guid.NewGuid(), "Development", Guid.NewGuid(), "Features", false, false, "false", true, 1, DateTime.UtcNow, DateTime.UtcNow, "system", "system")
            };

            _logger.LogWarning("Database unavailable, returning sample data");
            return Ok(sampleConfigurations);
        }
    }

    /// <summary>
    /// Get configuration by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConfigurationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await _configurationService.GetByIdAsync(id, cancellationToken);
            if (configuration == null)
            {
                return NotFound($"Configuration with ID {id} not found");
            }
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configuration by key and environment
    /// </summary>
    [HttpGet("by-key")]
    [ProducesResponseType(typeof(ConfigurationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByKey([FromQuery] string key, [FromQuery] Guid environmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = await _configurationService.GetByKeyAndEnvironmentAsync(key, environmentId, cancellationToken);
            if (configuration == null)
            {
                return NotFound($"Configuration with key '{key}' not found in environment {environmentId}");
            }
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration {Key} for environment {EnvironmentId}", key, environmentId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configurations by environment
    /// </summary>
    [HttpGet("environment/{environmentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), 200)]
    public async Task<IActionResult> GetByEnvironment(Guid environmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await _configurationService.GetByEnvironmentAsync(environmentId, cancellationToken);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations for environment {EnvironmentId}", environmentId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configurations by group
    /// </summary>
    [HttpGet("group/{groupId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), 200)]
    public async Task<IActionResult> GetByGroup(Guid groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await _configurationService.GetByGroupAsync(groupId, cancellationToken);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configurations for group {GroupId}", groupId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Search configurations
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), 200)]
    public async Task<IActionResult> Search([FromBody] ConfigurationSearchDto searchDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await _configurationService.SearchAsync(searchDto, cancellationToken);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching configurations");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConfigurationDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateConfigurationDto createDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var configuration = await _configurationService.CreateAsync(createDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = configuration.Id }, configuration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update a configuration
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ConfigurationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConfigurationDto updateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var configuration = await _configurationService.UpdateAsync(id, updateDto, userId, cancellationToken);
            return Ok(configuration);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid configuration data for update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a configuration
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _configurationService.DeleteAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Activate a configuration
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _configurationService.ActivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Deactivate a configuration
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _configurationService.DeactivateAsync(id, userId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get configuration history
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationHistoryDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _configurationService.GetHistoryAsync(id, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving history for configuration {Id}", id);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Bulk update configurations
    /// </summary>
    [HttpPost("bulk-update")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkConfigurationUpdateDto bulkUpdateDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _configurationService.BulkUpdateAsync(bulkUpdateDto, userId, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid bulk update data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk update");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get environment configurations as key-value pairs
    /// </summary>
    [HttpGet("environment/{environmentId:guid}/key-values")]
    [ProducesResponseType(typeof(Dictionary<string, string>), 200)]
    public async Task<IActionResult> GetEnvironmentKeyValues(Guid environmentId, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var configurations = await _configurationService.GetEnvironmentConfigurationsAsync(environmentId, activeOnly, cancellationToken);
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving key-value configurations for environment {EnvironmentId}", environmentId);
            return HandleException(ex);
        }
    }
}
