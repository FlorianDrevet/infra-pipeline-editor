using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListIncomingCrossConfigReferences;

/// <summary>
/// Query to list cross-config resource references from OTHER configurations
/// in the same project that target resources belonging to THIS configuration.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier whose resources are targeted.</param>
public record ListIncomingCrossConfigReferencesQuery(Guid InfraConfigId)
    : IQuery<List<IncomingCrossConfigReferenceResult>>;
