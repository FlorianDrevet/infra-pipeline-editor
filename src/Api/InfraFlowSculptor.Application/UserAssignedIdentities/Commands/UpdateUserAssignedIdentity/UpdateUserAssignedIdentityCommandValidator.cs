using FluentValidation;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.UpdateUserAssignedIdentity;

/// <summary>Validates the <see cref="UpdateUserAssignedIdentityCommand"/> before it is handled.</summary>
public sealed class UpdateUserAssignedIdentityCommandValidator : AbstractValidator<UpdateUserAssignedIdentityCommand>
{
    /// <summary>Initializes validation rules for updating a user-assigned identity.</summary>
    public UpdateUserAssignedIdentityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Name.Value)
            .MaximumLength(80).WithMessage("Name must not exceed 80 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}