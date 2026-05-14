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

        RuleForEach(x => x.EnvironmentSettings)
            .ChildRules(env =>
            {
                env.RuleFor(e => e.EnvironmentName)
                    .NotEmpty().WithMessage("EnvironmentName is required.");

                env.RuleFor(e => e.AddressSpaces)
                    .NotEmpty().WithMessage("At least one address space is required.");

                env.RuleForEach(e => e.AddressSpaces)
                    .NotEmpty().WithMessage("Address space must not be empty.")
                    .Matches(@"^\d{1,3}(\.\d{1,3}){3}/\d{1,2}$")
                    .WithMessage("Address space must be in CIDR notation (e.g. 10.0.0.0/16).");
            });
    }
}
