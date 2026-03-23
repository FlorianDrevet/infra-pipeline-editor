using FluentValidation;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.CreateUserAssignedIdentity;

/// <summary>
/// Validates the <see cref="CreateUserAssignedIdentityCommand"/> before it is handled.
/// </summary>
public sealed class CreateUserAssignedIdentityCommandValidator
    : AbstractValidator<CreateUserAssignedIdentityCommand>
{
    public CreateUserAssignedIdentityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(80).WithMessage("Name must not exceed 80 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");
    }
}
