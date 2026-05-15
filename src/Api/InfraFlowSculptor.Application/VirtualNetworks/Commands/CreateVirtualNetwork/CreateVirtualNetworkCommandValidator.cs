using FluentValidation;
using InfraFlowSculptor.Application.VirtualNetworks.Common;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.CreateVirtualNetwork;

/// <summary>Validates the <see cref="CreateVirtualNetworkCommand"/> before it is handled.</summary>
public sealed class CreateVirtualNetworkCommandValidator : AbstractValidator<CreateVirtualNetworkCommand>
{
    /// <summary>Initializes validation rules for creating a Virtual Network.</summary>
    public CreateVirtualNetworkCommandValidator()
    {
        RuleFor(x => x.ResourceGroupId)
            .NotEmpty().WithMessage("ResourceGroupId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.");

        VirtualNetworkEnvironmentSettingsRules.ApplyTo(this);
    }
}
