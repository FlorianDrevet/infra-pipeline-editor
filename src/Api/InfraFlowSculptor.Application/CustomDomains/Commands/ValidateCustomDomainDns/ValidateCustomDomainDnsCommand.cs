using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.ValidateCustomDomainDns;

/// <summary>Command to mark a custom domain's DNS records as validated.</summary>
/// <param name="ResourceId">Identifier of the parent Azure resource.</param>
/// <param name="CustomDomainId">Identifier of the custom domain to validate.</param>
public record ValidateCustomDomainDnsCommand(
    AzureResourceId ResourceId,
    CustomDomainId CustomDomainId) : ICommand<CustomDomainResult>;
