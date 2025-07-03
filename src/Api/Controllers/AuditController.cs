using Infrastructure.MongoDB.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiVersion("1.0")]
public class AuditController : BaseApiController
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get all audit logs with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<AuditLog> auditLogs;

            if (!string.IsNullOrEmpty(entityType) && entityId.HasValue)
            {
                auditLogs = await _auditService.GetEntityAuditLogsAsync(entityType, entityId.Value, limit, cancellationToken);
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                auditLogs = await _auditService.GetUserAuditLogsAsync(userId, limit, cancellationToken);
            }
            else
            {
                auditLogs = await _auditService.GetAllAuditLogsAsync(limit, cancellationToken);
            }

            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), 200)]
    public async Task<IActionResult> GetEntityAuditLogs(
        string entityType, 
        Guid entityId, 
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _auditService.GetEntityAuditLogsAsync(entityType, entityId, limit, cancellationToken);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for {EntityType} {EntityId}", entityType, entityId);
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get audit logs for a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), 200)]
    public async Task<IActionResult> GetUserAuditLogs(
        string userId, 
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _auditService.GetUserAuditLogsAsync(userId, limit, cancellationToken);
            return Ok(auditLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return HandleException(ex);
        }
    }
}
