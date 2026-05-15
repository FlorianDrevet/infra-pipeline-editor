using FluentValidation;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.UpdatePrivateDnsZone;

/// <summary>Validates the <see cref="UpdatePrivateDnsZoneCommand"/>.</summary>
public sealed class UpdatePrivateDnsZoneCommandValidator : AbstractValidator<UpdatePrivateDnsZoneCommand>
{
    public UpdatePrivateDnsZoneCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
