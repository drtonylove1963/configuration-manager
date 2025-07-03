using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class Permission : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { } // For EF Core

    public Permission(string name, string description, string resource, string action, string createdBy)
    {
        var validator = new PermissionValidator();
        var validationData = new PermissionValidationData 
        { 
            Name = name, 
            Description = description, 
            Resource = resource, 
            Action = action 
        };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid permission: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        Resource = resource;
        Action = action;
        CreatedBy = createdBy;
    }

    public void UpdateDetails(string name, string description, string resource, string action, string updatedBy)
    {
        var validator = new PermissionValidator();
        var validationData = new PermissionValidationData 
        { 
            Name = name, 
            Description = description, 
            Resource = resource, 
            Action = action 
        };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid permission: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        Resource = resource;
        Action = action;
        MarkAsUpdated(updatedBy);
    }

    public void Activate(string updatedBy)
    {
        IsActive = true;
        MarkAsUpdated(updatedBy);
    }

    public void Deactivate(string updatedBy)
    {
        IsActive = false;
        MarkAsUpdated(updatedBy);
    }
}

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public string AssignedBy { get; private set; } = string.Empty;
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { } // For EF Core

    public RolePermission(Guid roleId, Guid permissionId, string assignedBy)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
        CreatedBy = assignedBy;
    }
}

public class PermissionValidationData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}

public class PermissionValidator : AbstractValidator<PermissionValidationData>
{
    public PermissionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Permission name is required")
            .MaximumLength(100)
            .WithMessage("Permission name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Permission description cannot exceed 500 characters");

        RuleFor(x => x.Resource)
            .NotEmpty()
            .WithMessage("Resource is required")
            .MaximumLength(100)
            .WithMessage("Resource cannot exceed 100 characters");

        RuleFor(x => x.Action)
            .NotEmpty()
            .WithMessage("Action is required")
            .MaximumLength(50)
            .WithMessage("Action cannot exceed 50 characters");
    }
}

// Common permissions for the Configuration Manager
public static class Permissions
{
    // Configuration permissions
    public const string ConfigurationRead = "configuration.read";
    public const string ConfigurationCreate = "configuration.create";
    public const string ConfigurationUpdate = "configuration.update";
    public const string ConfigurationDelete = "configuration.delete";

    // Environment permissions
    public const string EnvironmentRead = "environment.read";
    public const string EnvironmentCreate = "environment.create";
    public const string EnvironmentUpdate = "environment.update";
    public const string EnvironmentDelete = "environment.delete";

    // Group permissions
    public const string GroupRead = "group.read";
    public const string GroupCreate = "group.create";
    public const string GroupUpdate = "group.update";
    public const string GroupDelete = "group.delete";

    // Audit permissions
    public const string AuditRead = "audit.read";

    // User management permissions
    public const string UserRead = "user.read";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";

    // Role management permissions
    public const string RoleRead = "role.read";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleDelete = "role.delete";

    // System administration
    public const string SystemAdmin = "system.admin";
}

// Common roles
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string ConfigurationManager = "Configuration Manager";
    public const string Viewer = "Viewer";
    public const string Auditor = "Auditor";
}
