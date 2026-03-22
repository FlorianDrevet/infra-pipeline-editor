using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.DeleteInfraConfig;

/// <summary>Command to delete an infrastructure configuration. Requires Owner access on the parent project.</summary>
public record DeleteInfrastructureConfigCommand(InfrastructureConfigId InfraConfigId)
    : IRequest<ErrorOr<Unit>>;
