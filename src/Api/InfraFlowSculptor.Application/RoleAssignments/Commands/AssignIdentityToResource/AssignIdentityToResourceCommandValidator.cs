using FluentValidation;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.AssignIdentityToResource;

/// <summary>Validates the <see cref="AssignIdentityToResourceCommand"/>.</summary>
public sealed class AssignIdentityToResourceCommandValidator : AbstractValidator<AssignIdentityToResourceCommand>
{
    /// <summary>Initializes a new instance of the <see cref="AssignIdentityToResourceCommandValidator"/> class.</summary>
    public AssignIdentityToResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull().WithMessage("ResourceId is required.");

        RuleFor(x => x.UserAssignedIdentityId)
            .NotNull().WithMessage("UserAssignedIdentityId is required.");
    }
}
