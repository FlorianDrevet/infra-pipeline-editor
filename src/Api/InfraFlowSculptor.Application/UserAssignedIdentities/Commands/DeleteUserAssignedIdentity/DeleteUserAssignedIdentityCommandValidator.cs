using FluentValidation;

namespace InfraFlowSculptor.Application.UserAssignedIdentities.Commands.DeleteUserAssignedIdentity;

/// <summary>Validates the <see cref="DeleteUserAssignedIdentityCommand"/>.</summary>
public sealed class DeleteUserAssignedIdentityCommandValidator : AbstractValidator<DeleteUserAssignedIdentityCommand>
{
    public DeleteUserAssignedIdentityCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
