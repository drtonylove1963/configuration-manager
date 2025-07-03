using Domain.Entities;
using Infrastructure.MongoDB;
using Infrastructure.MongoDB.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Services;

public interface IConfigurationCacheService
{
    Task<Dictionary<string, string>> GetEnvironmentConfigurationsAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task<string?> GetConfigurationValueAsync(string key, Guid environmentId, CancellationToken cancellationToken = default);
    Task CacheConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default);
    Task InvalidateConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default);
    Task InvalidateEnvironmentCacheAsync(Guid environmentId, CancellationToken cancellationToken = default);
    Task RefreshCacheAsync(IEnumerable<Configuration> configurations, CancellationToken cancellationToken = default);
}

public class ConfigurationCacheService : IConfigurationCacheService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<ConfigurationCacheService> _logger;

    public ConfigurationCacheService(MongoDbContext mongoContext, ILogger<ConfigurationCacheService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task<Dictionary<string, string>> GetEnvironmentConfigurationsAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<ConfigurationCache>.Filter.And(
                Builders<ConfigurationCache>.Filter.Eq(x => x.EnvironmentId, environmentId),
                Builders<ConfigurationCache>.Filter.Eq(x => x.IsActive, true),
                Builders<ConfigurationCache>.Filter.Gt(x => x.TTL, DateTime.UtcNow)
            );

            var cachedConfigs = await _mongoContext.ConfigurationCache
                .Find(filter)
                .ToListAsync(cancellationToken);

            var result = cachedConfigs.ToDictionary(c => c.Key, c => c.Value);
            
            _logger.LogDebug("Retrieved {Count} cached configurations for environment {EnvironmentId}", 
                result.Count, environmentId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached configurations for environment {EnvironmentId}", environmentId);
            return new Dictionary<string, string>();
        }
    }

    public async Task<string?> GetConfigurationValueAsync(string key, Guid environmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<ConfigurationCache>.Filter.And(
                Builders<ConfigurationCache>.Filter.Eq(x => x.Key, key),
                Builders<ConfigurationCache>.Filter.Eq(x => x.EnvironmentId, environmentId),
                Builders<ConfigurationCache>.Filter.Eq(x => x.IsActive, true),
                Builders<ConfigurationCache>.Filter.Gt(x => x.TTL, DateTime.UtcNow)
            );

            var cachedConfig = await _mongoContext.ConfigurationCache
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            if (cachedConfig != null)
            {
                _logger.LogDebug("Cache hit for configuration {Key} in environment {EnvironmentId}", key, environmentId);
                return cachedConfig.Value;
            }

            _logger.LogDebug("Cache miss for configuration {Key} in environment {EnvironmentId}", key, environmentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached configuration {Key} for environment {EnvironmentId}", key, environmentId);
            return null;
        }
    }

    public async Task CacheConfigurationAsync(Configuration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheEntry = new ConfigurationCache
            {
                ConfigurationId = configuration.Id,
                EnvironmentId = configuration.EnvironmentId,
                EnvironmentName = configuration.Environment?.Name ?? string.Empty,
                Key = configuration.Key.Value,
                Value = configuration.Value.Value,
                ValueType = configuration.Value.Type.ToString(),
                IsActive = configuration.IsActive,
                IsEncrypted = configuration.IsEncrypted,
                GroupId = configuration.GroupId,
                GroupName = configuration.Group?.Name,
                LastUpdated = configuration.UpdatedAt ?? configuration.CreatedAt,
                CachedAt = DateTime.UtcNow,
                TTL = DateTime.UtcNow.AddHours(1)
            };

            var filter = Builders<ConfigurationCache>.Filter.Eq(x => x.ConfigurationId, configuration.Id);
            
            await _mongoContext.ConfigurationCache.ReplaceOneAsync(
                filter, 
                cacheEntry, 
                new ReplaceOptions { IsUpsert = true }, 
                cancellationToken);

            _logger.LogDebug("Cached configuration {Key} for environment {EnvironmentId}", 
                configuration.Key.Value, configuration.EnvironmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching configuration {Key} for environment {EnvironmentId}", 
                configuration.Key.Value, configuration.EnvironmentId);
        }
    }

    public async Task InvalidateConfigurationAsync(Guid configurationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<ConfigurationCache>.Filter.Eq(x => x.ConfigurationId, configurationId);
            await _mongoContext.ConfigurationCache.DeleteOneAsync(filter, cancellationToken);
            
            _logger.LogDebug("Invalidated cache for configuration {ConfigurationId}", configurationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for configuration {ConfigurationId}", configurationId);
        }
    }

    public async Task InvalidateEnvironmentCacheAsync(Guid environmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<ConfigurationCache>.Filter.Eq(x => x.EnvironmentId, environmentId);
            await _mongoContext.ConfigurationCache.DeleteManyAsync(filter, cancellationToken);
            
            _logger.LogDebug("Invalidated cache for environment {EnvironmentId}", environmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for environment {EnvironmentId}", environmentId);
        }
    }

    public async Task RefreshCacheAsync(IEnumerable<Configuration> configurations, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = configurations.Select(config => CacheConfigurationAsync(config, cancellationToken));
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Refreshed cache for {Count} configurations", configurations.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configuration cache");
        }
    }
}
