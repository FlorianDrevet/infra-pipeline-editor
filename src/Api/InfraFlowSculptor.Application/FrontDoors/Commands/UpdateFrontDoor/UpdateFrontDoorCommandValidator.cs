using FluentValidation;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.UpdateFrontDoor;

/// <summary>Validates the <see cref="UpdateFrontDoorCommand"/>.</summary>
public sealed class UpdateFrontDoorCommandValidator : AbstractValidator<UpdateFrontDoorCommand>
{
    public UpdateFrontDoorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
