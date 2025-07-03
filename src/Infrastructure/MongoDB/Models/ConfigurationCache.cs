using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.MongoDB.Models;

public class ConfigurationCache
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("configurationId")]
    public Guid ConfigurationId { get; set; }

    [BsonElement("environmentId")]
    public Guid EnvironmentId { get; set; }

    [BsonElement("environmentName")]
    public string EnvironmentName { get; set; } = string.Empty;

    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;

    [BsonElement("valueType")]
    public string ValueType { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    [BsonElement("isEncrypted")]
    public bool IsEncrypted { get; set; }

    [BsonElement("groupId")]
    public Guid? GroupId { get; set; }

    [BsonElement("groupName")]
    public string? GroupName { get; set; }

    [BsonElement("lastUpdated")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastUpdated { get; set; }

    [BsonElement("cachedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("ttl")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime TTL { get; set; } = DateTime.UtcNow.AddHours(1); // 1 hour TTL
}

public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [BsonElement("entityId")]
    public Guid EntityId { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("changes")]
    public Dictionary<string, object> Changes { get; set; } = new();

    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }
}
