using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Queries.GetDnsInstructions;

/// <summary>Query to retrieve DNS configuration instructions for a custom domain.</summary>
/// <param name="ResourceId">Identifier of the parent Azure resource.</param>
/// <param name="CustomDomainId">Identifier of the custom domain.</param>
public record GetDnsInstructionsQuery(
    AzureResourceId ResourceId,
    CustomDomainId CustomDomainId) : IQuery<DnsInstructionsResult>;
