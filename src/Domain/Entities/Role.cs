using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyList<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { } // For EF Core

    public Role(string name, string description, string createdBy)
    {
        var validator = new RoleValidator();
        var validationData = new RoleValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid role: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
        CreatedBy = createdBy;
    }

    public void UpdateDetails(string name, string description, string updatedBy)
    {
        var validator = new RoleValidator();
        var validationData = new RoleValidationData { Name = name, Description = description };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid role: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        Name = name;
        Description = description;
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

    public void AddPermission(Permission permission, string assignedBy)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        _rolePermissions.Add(new RolePermission(Id, permission.Id, assignedBy));
    }

    public void RemovePermission(Guid permissionId)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _rolePermissions.Remove(rolePermission);
        }
    }
}

public class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public string AssignedBy { get; private set; } = string.Empty;
    public DateTime AssignedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { } // For EF Core

    public UserRole(Guid userId, Guid roleId, string assignedBy)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
        CreatedBy = assignedBy;
    }
}

public class RoleValidationData
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RoleValidator : AbstractValidator<RoleValidationData>
{
    public RoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Role name is required")
            .MaximumLength(100)
            .WithMessage("Role name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\s._-]+$")
            .WithMessage("Role name can only contain letters, numbers, spaces, dots, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Role description cannot exceed 500 characters");
    }
}
