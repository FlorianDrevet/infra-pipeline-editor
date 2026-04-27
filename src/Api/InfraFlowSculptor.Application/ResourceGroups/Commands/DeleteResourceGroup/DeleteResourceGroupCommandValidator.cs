using FluentValidation;

namespace InfraFlowSculptor.Application.ResourceGroups.Commands.DeleteResourceGroup;

/// <summary>Validates the <see cref="DeleteResourceGroupCommand"/>.</summary>
public sealed class DeleteResourceGroupCommandValidator : AbstractValidator<DeleteResourceGroupCommand>
{
    public DeleteResourceGroupCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
