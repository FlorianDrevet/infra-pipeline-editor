using FluentValidation;
using InfraFlowSculptor.Domain.NetworkingProfileAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.NetworkingProfiles.Commands.SetNetworkingProfile;

/// <summary>Validates <see cref="SetNetworkingProfileCommand"/> inputs.</summary>
public sealed class SetNetworkingProfileCommandValidator : AbstractValidator<SetNetworkingProfileCommand>
{
    public SetNetworkingProfileCommandValidator()
    {
        RuleFor(x => x.InfraConfigId).NotNull();
        RuleFor(x => x.Mode).IsInEnum();
        RuleFor(x => x.VnetSourceType).IsInEnum();
        RuleFor(x => x.DnsMode).IsInEnum();

        // CreateNew requires address spaces
        When(x => x.VnetSourceType == VnetSource.SourceType.CreateNew, () =>
        {
            RuleFor(x => x.CreateNewAddressSpace)
                .NotEmpty()
                .WithMessage("Address space is required when creating a new VNet.");

            RuleFor(x => x.CreateNewSubnetAddressPrefix)
                .NotEmpty()
                .WithMessage("Subnet address prefix is required when creating a new VNet.");
        });

        // UseExisting/UseHubSpoke requires existing VNet resource ID
        When(x => x.VnetSourceType is VnetSource.SourceType.UseExisting or VnetSource.SourceType.UseHubSpoke, () =>
        {
            RuleFor(x => x.ExistingVnetResourceId)
                .NotEmpty()
                .WithMessage("Existing VNet resource ID is required.");
        });

        // CentralizedHub DNS requires hub details
        When(x => x.DnsMode == DnsMode.Mode.CentralizedHub, () =>
        {
            RuleFor(x => x.DnsHubResourceGroupId)
                .NotEmpty()
                .WithMessage("Hub resource group ID is required for centralized DNS.");

            RuleFor(x => x.DnsHubSubscriptionId)
                .NotEmpty()
                .WithMessage("Hub subscription ID is required for centralized DNS.");
        });
    }
}
