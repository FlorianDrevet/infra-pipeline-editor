using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Domain.ProjectAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.Projects.Commands.SetAgentPool;

/// <summary>Command to set or clear the self-hosted agent pool name for pipeline generation.</summary>
public record SetAgentPoolCommand(
    ProjectId ProjectId,
    string? AgentPoolName
) : ICommand<Success>;
