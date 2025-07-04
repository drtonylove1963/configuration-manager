using Domain.Common;
using FluentValidation;

namespace Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public Guid ApplicationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public string AssignedBy { get; private set; } = string.Empty;
    public DateTime AssignedAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastAccessedAt { get; private set; }

    // Navigation properties
    public Application Application { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private ApplicationUser() { } // For EF Core

    public ApplicationUser(Guid applicationId, Guid userId, Guid roleId, string assignedBy)
    {
        var validator = new ApplicationUserValidator();
        var validationData = new ApplicationUserValidationData
        {
            ApplicationId = applicationId,
            UserId = userId,
            RoleId = roleId,
            AssignedBy = assignedBy
        };
        var validationResult = validator.Validate(validationData);

        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Invalid application user assignment: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
        }

        ApplicationId = applicationId;
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
        CreatedBy = assignedBy;
    }

    public void ChangeRole(Guid newRoleId, string updatedBy)
    {
        RoleId = newRoleId;
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

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    public void Delete(string deletedBy)
    {
        MarkAsDeleted(deletedBy);
    }
}

public class ApplicationUserValidator : AbstractValidator<ApplicationUserValidationData>
{
    public ApplicationUserValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required");

        RuleFor(x => x.AssignedBy)
            .NotEmpty()
            .WithMessage("Assigned by is required")
            .MaximumLength(100)
            .WithMessage("Assigned by cannot exceed 100 characters");
    }
}

public class ApplicationUserValidationData
{
    public Guid ApplicationId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
}