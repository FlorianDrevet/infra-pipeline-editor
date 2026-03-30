using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.UpdateFunctionApp;

/// <summary>Command to update an existing Function App.</summary>
public record UpdateFunctionAppCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? DockerImageName,
    IReadOnlyList<FunctionAppEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<FunctionAppResult>;
