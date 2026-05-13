using FluentValidation;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.RemoveRoleAssignment;

/// <summary>Validates the <see cref="RemoveRoleAssignmentCommand"/> before it is handled.</summary>
public sealed class RemoveRoleAssignmentCommandValidator : AbstractValidator<RemoveRoleAssignmentCommand>
{
    /// <summary>Initializes validation rules for removing a role assignment.</summary>
    public RemoveRoleAssignmentCommandValidator()
    {
        RuleFor(x => x.SourceResourceId)
            .NotEmpty().WithMessage("SourceResourceId is required.");

        RuleFor(x => x.RoleAssignmentId)
            .NotEmpty().WithMessage("RoleAssignmentId is required.");
    }
}