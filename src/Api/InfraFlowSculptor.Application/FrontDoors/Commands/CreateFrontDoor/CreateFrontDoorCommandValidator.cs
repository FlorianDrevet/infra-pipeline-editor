using FluentValidation;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.CreateFrontDoor;

/// <summary>Validates the <see cref="CreateFrontDoorCommand"/>.</summary>
public sealed class CreateFrontDoorCommandValidator : AbstractValidator<CreateFrontDoorCommand>
{
    public CreateFrontDoorCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("ResourceGroupId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
