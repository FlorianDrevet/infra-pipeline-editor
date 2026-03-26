using ErrorOr;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.AddCrossConfigReference;

/// <summary>
/// Command to add a cross-configuration resource reference to an infrastructure configuration.
/// The target resource must belong to another configuration within the same project.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration to add the reference to.</param>
/// <param name="TargetResourceId">The target Azure resource to reference.</param>
public record AddCrossConfigReferenceCommand(Guid InfraConfigId, Guid TargetResourceId)
    : IRequest<ErrorOr<CrossConfigReferenceResult>>;
