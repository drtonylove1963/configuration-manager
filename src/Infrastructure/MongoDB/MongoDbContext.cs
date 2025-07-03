using Infrastructure.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.MongoDB;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConfigurationCacheCollection { get; set; } = "ConfigurationCache";
    public string AuditLogCollection { get; set; } = "AuditLogs";
}

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        
        ConfigurationCacheCollectionName = settings.Value.ConfigurationCacheCollection;
        AuditLogCollectionName = settings.Value.AuditLogCollection;
        
        // Create indexes
        CreateIndexes();
    }

    public string ConfigurationCacheCollectionName { get; }
    public string AuditLogCollectionName { get; }

    public IMongoCollection<ConfigurationCache> ConfigurationCache =>
        _database.GetCollection<ConfigurationCache>(ConfigurationCacheCollectionName);

    public IMongoCollection<AuditLog> AuditLogs =>
        _database.GetCollection<AuditLog>(AuditLogCollectionName);

    private void CreateIndexes()
    {
        // Configuration Cache indexes
        var cacheIndexes = new[]
        {
            new CreateIndexModel<ConfigurationCache>(
                Builders<ConfigurationCache>.IndexKeys
                    .Ascending(x => x.EnvironmentId)
                    .Ascending(x => x.Key)),
            new CreateIndexModel<ConfigurationCache>(
                Builders<ConfigurationCache>.IndexKeys.Ascending(x => x.ConfigurationId)),
            new CreateIndexModel<ConfigurationCache>(
                Builders<ConfigurationCache>.IndexKeys.Ascending(x => x.TTL),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }),
            new CreateIndexModel<ConfigurationCache>(
                Builders<ConfigurationCache>.IndexKeys.Ascending(x => x.IsActive)),
            new CreateIndexModel<ConfigurationCache>(
                Builders<ConfigurationCache>.IndexKeys.Ascending(x => x.GroupId))
        };

        ConfigurationCache.Indexes.CreateMany(cacheIndexes);

        // Audit Log indexes
        var auditIndexes = new[]
        {
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Descending(x => x.Timestamp)),
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys
                    .Ascending(x => x.EntityType)
                    .Ascending(x => x.EntityId)),
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(x => x.UserId)),
            new CreateIndexModel<AuditLog>(
                Builders<AuditLog>.IndexKeys.Ascending(x => x.Action))
        };

        AuditLogs.Indexes.CreateMany(auditIndexes);
    }
}
