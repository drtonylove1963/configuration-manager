using Domain.ValueObjects;

namespace Domain.Events;

public record ConfigurationChangedEvent(
    Guid ConfigurationId,
    string Key,
    string OldValue,
    string NewValue,
    ConfigurationValueType ValueType,
    string Environment,
    string ChangedBy,
    DateTime ChangedAt,
    string? ChangeReason = null);

public record ConfigurationCreatedEvent(
    Guid ConfigurationId,
    string Key,
    string Value,
    ConfigurationValueType ValueType,
    string Environment,
    string CreatedBy,
    DateTime CreatedAt);

public record ConfigurationDeletedEvent(
    Guid ConfigurationId,
    string Key,
    string Environment,
    string DeletedBy,
    DateTime DeletedAt);
