using Infrastructure.MongoDB;
using Infrastructure.MongoDB.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(string entityType, Guid entityId, string action, string userId, string userName,
        Dictionary<string, object>? changes = null, Dictionary<string, object>? metadata = null,
        string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

    Task LogConfigurationChangeAsync(Guid configurationId, string action, string userId, string userName,
        string? oldValue = null, string? newValue = null, string? changeReason = null,
        string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int limit = 100, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId,
        int limit = 100, CancellationToken cancellationToken = default);

    Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId,
        int limit = 100, CancellationToken cancellationToken = default);
}

public class AuditService : IAuditService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(MongoDbContext mongoContext, ILogger<AuditService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task LogAsync(string entityType, Guid entityId, string action, string userId, string userName,
        Dictionary<string, object>? changes = null, Dictionary<string, object>? metadata = null,
        string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                UserId = userId,
                UserName = userName,
                Changes = changes ?? new Dictionary<string, object>(),
                Metadata = metadata ?? new Dictionary<string, object>(),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            await _mongoContext.AuditLogs.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
            
            _logger.LogDebug("Audit log created for {EntityType} {EntityId} - {Action} by {UserName}", 
                entityType, entityId, action, userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit log for {EntityType} {EntityId} - {Action}", 
                entityType, entityId, action);
        }
    }

    public async Task LogConfigurationChangeAsync(Guid configurationId, string action, string userId, string userName,
        string? oldValue = null, string? newValue = null, string? changeReason = null,
        string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default)
    {
        var changes = new Dictionary<string, object>();
        
        if (oldValue != null)
            changes["oldValue"] = oldValue;
        
        if (newValue != null)
            changes["newValue"] = newValue;

        var metadata = new Dictionary<string, object>();
        
        if (changeReason != null)
            metadata["changeReason"] = changeReason;

        await LogAsync("Configuration", configurationId, action, userId, userName, 
            changes, metadata, ipAddress, userAgent, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLogs = await _mongoContext.AuditLogs
                .Find(Builders<AuditLog>.Filter.Empty)
                .SortByDescending(x => x.Timestamp)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all audit logs");
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetEntityAuditLogsAsync(string entityType, Guid entityId,
        int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<AuditLog>.Filter.And(
                Builders<AuditLog>.Filter.Eq(x => x.EntityType, entityType),
                Builders<AuditLog>.Filter.Eq(x => x.EntityId, entityId)
            );

            var auditLogs = await _mongoContext.AuditLogs
                .Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for {EntityType} {EntityId}", entityType, entityId);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetUserAuditLogsAsync(string userId,
        int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<AuditLog>.Filter.Eq(x => x.UserId, userId);

            var auditLogs = await _mongoContext.AuditLogs
                .Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Limit(limit)
                .ToListAsync(cancellationToken);

            return auditLogs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {UserId}", userId);
            return Enumerable.Empty<AuditLog>();
        }
    }
}
