using FluentValidation;

namespace InfraFlowSculptor.Application.RoleAssignments.Commands.UnassignIdentityFromResource;

/// <summary>Validates the <see cref="UnassignIdentityFromResourceCommand"/>.</summary>
public sealed class UnassignIdentityFromResourceCommandValidator : AbstractValidator<UnassignIdentityFromResourceCommand>
{
    /// <summary>Initializes a new instance of the <see cref="UnassignIdentityFromResourceCommandValidator"/> class.</summary>
    public UnassignIdentityFromResourceCommandValidator()
    {
        RuleFor(x => x.ResourceId)
            .NotNull().WithMessage("ResourceId is required.");
    }
}
