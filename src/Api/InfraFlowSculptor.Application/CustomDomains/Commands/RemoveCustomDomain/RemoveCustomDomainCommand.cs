using ErrorOr;
using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Commands.RemoveCustomDomain;

/// <summary>Command to remove a custom domain binding from a compute resource.</summary>
/// <param name="ResourceId">Identifier of the compute resource.</param>
/// <param name="CustomDomainId">Identifier of the custom domain binding to remove.</param>
public record RemoveCustomDomainCommand(
    AzureResourceId ResourceId,
    CustomDomainId CustomDomainId) : ICommand<Deleted>;
