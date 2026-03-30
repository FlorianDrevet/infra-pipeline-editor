using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetInheritance;

/// <summary>Command to toggle project-level inheritance for naming conventions.</summary>
public record SetInheritanceCommand(
    InfrastructureConfigId InfraConfigId,
    bool UseProjectNamingConventions
) : ICommand<Success>;
