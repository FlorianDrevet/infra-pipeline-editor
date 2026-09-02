using FluentValidation;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetPrivateEndpointConfig;

/// <summary>Validates <see cref="SetPrivateEndpointConfigCommand"/> inputs.</summary>
public sealed class SetPrivateEndpointConfigCommandValidator : AbstractValidator<SetPrivateEndpointConfigCommand>
{
    public SetPrivateEndpointConfigCommandValidator()
    {
        RuleFor(x => x.InfraConfigId).NotNull();
        RuleFor(x => x.ResourceId).NotNull();
        RuleFor(x => x.VirtualNetworkId).NotNull();
        RuleFor(x => x.SubnetName).NotEmpty();

        RuleFor(x => x.DnsMode)
            .NotEmpty()
            .Must(BeAValidDnsMode)
            .WithMessage("DnsMode must be one of: AutoManaged, ExistingHub, Disabled.");

        When(x => x.DnsMode == nameof(PrivateEndpointDnsMode.Mode.ExistingHub), () =>
        {
            RuleFor(x => x.DnsHubResourceGroupId).NotEmpty()
                .WithMessage("DnsHubResourceGroupId is required when DnsMode is ExistingHub.");
            RuleFor(x => x.DnsHubSubscriptionId).NotEmpty()
                .WithMessage("DnsHubSubscriptionId is required when DnsMode is ExistingHub.");
        });
    }

    private static bool BeAValidDnsMode(string dnsMode)
    {
        return Enum.TryParse<PrivateEndpointDnsMode.Mode>(dnsMode, ignoreCase: true, out _);
    }
}
