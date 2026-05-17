using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.FunctionApps.Common;
using InfraFlowSculptor.Contracts.Common.Requests;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;

namespace InfraFlowSculptor.Application.FunctionApps.Commands.CreateFunctionApp;

/// <summary>Command to create a new Function App inside a Resource Group.</summary>
public record CreateFunctionAppCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? AcrAuthMode,
    Guid? AcrPullIdentityId,
    string? DockerImageName,
    bool DockerImageValidated = false,
    string? DockerfilePath = null,
    string? SourceCodePath = null,
    string? BuildCommand = null,
    string? ApplicationName = null,
    IReadOnlyList<FunctionAppEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false,
    PipelineStepOptionsDto? PipelineStepOptions = null
) : ICommand<FunctionAppResult>;
