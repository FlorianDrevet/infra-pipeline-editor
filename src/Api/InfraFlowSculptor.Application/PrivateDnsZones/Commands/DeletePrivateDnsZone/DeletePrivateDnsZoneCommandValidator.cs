using FluentValidation;

namespace InfraFlowSculptor.Application.PrivateDnsZones.Commands.DeletePrivateDnsZone;

/// <summary>Validates the <see cref="DeletePrivateDnsZoneCommand"/>.</summary>
public sealed class DeletePrivateDnsZoneCommandValidator : AbstractValidator<DeletePrivateDnsZoneCommand>
{
    public DeletePrivateDnsZoneCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
