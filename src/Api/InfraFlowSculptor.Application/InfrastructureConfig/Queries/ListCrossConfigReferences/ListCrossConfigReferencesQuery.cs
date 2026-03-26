using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Queries.ListCrossConfigReferences;

/// <summary>Query to list all cross-configuration resource references for an infrastructure configuration.</summary>
/// <param name="InfraConfigId">The infrastructure configuration identifier.</param>
public record ListCrossConfigReferencesQuery(Guid InfraConfigId)
    : IRequest<ErrorOr<List<CrossConfigReferenceDetailResult>>>;
