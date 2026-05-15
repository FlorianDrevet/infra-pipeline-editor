using FluentValidation;

namespace InfraFlowSculptor.Application.FrontDoors.Commands.DeleteFrontDoor;

/// <summary>Validates the <see cref="DeleteFrontDoorCommand"/>.</summary>
public sealed class DeleteFrontDoorCommandValidator : AbstractValidator<DeleteFrontDoorCommand>
{
    public DeleteFrontDoorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
