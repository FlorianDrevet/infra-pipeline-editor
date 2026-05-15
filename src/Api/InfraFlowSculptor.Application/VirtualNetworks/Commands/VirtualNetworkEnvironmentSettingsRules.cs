using FluentValidation;
using InfraFlowSculptor.Application.VirtualNetworks.Common;

namespace InfraFlowSculptor.Application.VirtualNetworks.Common;

/// <summary>Shared FluentValidation rules for VNet environment settings.</summary>
internal static class VirtualNetworkEnvironmentSettingsRules
{
    /// <summary>Applies standard validation rules for environment settings collections.</summary>
    public static void ApplyTo<T>(AbstractValidator<T> validator)
        where T : IHasEnvironmentSettings
    {
        validator.RuleForEach(x => x.EnvironmentSettings)
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

                env.RuleForEach(e => e.DnsServers)
                    .NotEmpty().WithMessage("DNS server must not be empty.")
                    .Matches(@"^\d{1,3}(\.\d{1,3}){3}$")
                    .WithMessage("DNS server must be a valid IPv4 address (e.g. 10.0.0.4).");
            });
    }
}
