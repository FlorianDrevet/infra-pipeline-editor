using FluentValidation;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.RemoveSubnet;

/// <summary>Validates the <see cref="RemoveSubnetCommand"/> before it is handled.</summary>
public sealed class RemoveSubnetCommandValidator : AbstractValidator<RemoveSubnetCommand>
{
    /// <summary>Initializes validation rules for removing a subnet.</summary>
    public RemoveSubnetCommandValidator()
    {
        RuleFor(x => x.VirtualNetworkId)
            .NotEmpty().WithMessage("VirtualNetworkId is required.");

        RuleFor(x => x.SubnetId)
            .NotEmpty().WithMessage("SubnetId is required.");
    }
}
