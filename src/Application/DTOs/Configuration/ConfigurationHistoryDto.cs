using Domain.ValueObjects;

namespace Application.DTOs.Configuration;

public record ConfigurationHistoryDto(
    Guid Id,
    Guid ConfigurationId,
    string OldValue,
    ConfigurationValueType OldValueType,
    string NewValue,
    ConfigurationValueType NewValueType,
    string ChangedBy,
    string? ChangeReason,
    DateTime ChangedAt);
