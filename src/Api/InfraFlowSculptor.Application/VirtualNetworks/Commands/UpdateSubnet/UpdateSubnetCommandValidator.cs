using FluentValidation;

namespace InfraFlowSculptor.Application.VirtualNetworks.Commands.UpdateSubnet;

/// <summary>Validates the <see cref="UpdateSubnetCommand"/> before it is handled.</summary>
public sealed class UpdateSubnetCommandValidator : AbstractValidator<UpdateSubnetCommand>
{
    /// <summary>Initializes validation rules for updating a subnet.</summary>
    public UpdateSubnetCommandValidator()
    {
        RuleFor(x => x.VirtualNetworkId)
            .NotEmpty().WithMessage("VirtualNetworkId is required.");

        RuleFor(x => x.SubnetId)
            .NotEmpty().WithMessage("SubnetId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(80).WithMessage("Name must not exceed 80 characters.");

        RuleFor(x => x.AddressPrefix)
            .NotEmpty().WithMessage("AddressPrefix is required.")
            .Matches(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}/\d{1,2}$")
            .WithMessage("AddressPrefix must be a valid CIDR notation (e.g. 10.0.1.0/24).");

        RuleFor(x => x.PrivateEndpointNetworkPolicies)
            .NotEmpty().WithMessage("PrivateEndpointNetworkPolicies is required.");
    }
}
