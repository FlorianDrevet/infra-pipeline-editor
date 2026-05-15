using FluentValidation;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.CreatePrivateDnsZone;

/// <summary>Validates the <see cref="CreatePrivateDnsZoneCommand"/>.</summary>
public sealed class CreatePrivateDnsZoneCommandValidator : AbstractValidator<CreatePrivateDnsZoneCommand>
{
    public CreatePrivateDnsZoneCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId).NotEmpty().WithMessage("ResourceGroupId is required.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Location).NotEmpty().WithMessage("Location is required.");
    }
}
