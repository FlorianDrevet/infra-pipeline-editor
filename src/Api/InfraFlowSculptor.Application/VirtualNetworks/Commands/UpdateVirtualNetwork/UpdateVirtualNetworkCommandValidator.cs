using FluentValidation;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateVirtualNetwork;

/// <summary>Validates the <see cref="UpdateVirtualNetworkCommand"/> before it is handled.</summary>
public sealed class UpdateVirtualNetworkCommandValidator : AbstractValidator<UpdateVirtualNetworkCommand>
{
    /// <summary>Initializes validation rules for updating a Virtual Network.</summary>
    public UpdateVirtualNetworkCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");
    }
}
