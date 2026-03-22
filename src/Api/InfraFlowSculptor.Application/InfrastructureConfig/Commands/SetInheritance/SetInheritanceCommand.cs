using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;
using MediatR;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;

/// <summary>Command to toggle project-level inheritance for environments and/or naming conventions.</summary>
public record SetInheritanceCommand(
    InfrastructureConfigId InfraConfigId,
    bool UseProjectEnvironments,
    bool UseProjectNamingConventions
) : IRequest<ErrorOr<Success>>;
