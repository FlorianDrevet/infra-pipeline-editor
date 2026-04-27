using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.CustomDomains.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.CustomDomains.Queries.ListCustomDomains;

/// <summary>Query to list all custom domain bindings for a resource.</summary>
/// <param name="ResourceId">Identifier of the Azure resource.</param>
public record ListCustomDomainsQuery(AzureResourceId ResourceId)
    : IQuery<IReadOnlyList<CustomDomainResult>>;
