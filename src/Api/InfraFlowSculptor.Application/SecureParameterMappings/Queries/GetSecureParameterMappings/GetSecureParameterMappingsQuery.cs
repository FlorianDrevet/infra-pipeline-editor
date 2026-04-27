using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.SecureParameterMappings.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;

namespace InfraFlowSculptor.Application.SecureParameterMappings.Queries.GetSecureParameterMappings;

/// <summary>Query to list all secure parameter mappings for a resource.</summary>
/// <param name="ResourceId">Identifier of the Azure resource.</param>
public record GetSecureParameterMappingsQuery(AzureResourceId ResourceId)
    : IQuery<IReadOnlyList<SecureParameterMappingResult>>;
