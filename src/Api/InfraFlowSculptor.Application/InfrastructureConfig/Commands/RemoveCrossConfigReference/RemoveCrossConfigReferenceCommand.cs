using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.RemoveCrossConfigReference;

/// <summary>
/// Command to remove a cross-configuration resource reference from an infrastructure configuration.
/// </summary>
/// <param name="InfraConfigId">The infrastructure configuration that owns the reference.</param>
/// <param name="ReferenceId">The identifier of the cross-config reference to remove.</param>
public record RemoveCrossConfigReferenceCommand(Guid InfraConfigId, Guid ReferenceId)
    : ICommand<Deleted>;
