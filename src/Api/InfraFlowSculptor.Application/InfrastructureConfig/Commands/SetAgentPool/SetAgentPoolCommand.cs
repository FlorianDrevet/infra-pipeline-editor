using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.InfrastructureConfigAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.SetAgentPool;

/// <summary>Command to set or clear the self-hosted agent pool name for pipeline generation.</summary>
public record SetAgentPoolCommand(
    InfrastructureConfigId InfraConfigId,
    string? AgentPoolName
) : ICommand<Success>;
