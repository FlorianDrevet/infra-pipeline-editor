using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.InfrastructureConfig.Common;

namespace InfraFlowSculptor.Application.InfrastructureConfig.Commands.CreateInfraConfig;

/// <summary>Command to create a new infrastructure configuration within a project.</summary>
public record CreateInfrastructureConfigCommand(string Name, Guid ProjectId) : ICommand<GetInfrastructureConfigResult>;