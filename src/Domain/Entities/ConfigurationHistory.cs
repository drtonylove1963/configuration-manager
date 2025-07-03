using Domain.Common;
using Domain.ValueObjects;

namespace Domain.Entities;

public class ConfigurationHistory : BaseEntity
{
    public Guid ConfigurationId { get; private set; }
    public string OldValue { get; private set; } = string.Empty;
    public ConfigurationValueType OldValueType { get; private set; }
    public string NewValue { get; private set; } = string.Empty;
    public ConfigurationValueType NewValueType { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public string? ChangeReason { get; private set; }
    public DateTime ChangedAt { get; private set; }

    // Navigation property
    public Configuration Configuration { get; private set; } = null!;

    private ConfigurationHistory() { } // For EF Core

    public ConfigurationHistory(
        Guid configurationId,
        string oldValue,
        ConfigurationValueType oldValueType,
        string newValue,
        ConfigurationValueType newValueType,
        string changedBy,
        string? changeReason = null)
    {
        ConfigurationId = configurationId;
        OldValue = oldValue;
        OldValueType = oldValueType;
        NewValue = newValue;
        NewValueType = newValueType;
        ChangedBy = changedBy;
        ChangeReason = changeReason;
        ChangedAt = DateTime.UtcNow;
        CreatedBy = changedBy;
    }
}
