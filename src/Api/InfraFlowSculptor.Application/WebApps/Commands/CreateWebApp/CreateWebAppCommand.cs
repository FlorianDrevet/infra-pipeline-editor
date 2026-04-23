using InfraFlowSculptor.Application.Common.Interfaces;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.ValueObjects;
using InfraFlowSculptor.Domain.ResourceGroupAggregate.ValueObjects;
using ErrorOr;

namespace InfraFlowSculptor.Application.WebApps.Commands.CreateWebApp;

/// <summary>Command to create a new Web App inside a Resource Group.</summary>
public record CreateWebAppCommand(
    ResourceGroupId ResourceGroupId,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? AcrAuthMode,
    string? DockerImageName,
    string? DockerfilePath = null,
    string? SourceCodePath = null,
    string? BuildCommand = null,
    string? ApplicationName = null,
    IReadOnlyList<WebAppEnvironmentConfigData>? EnvironmentSettings = null,
    bool IsExisting = false
) : ICommand<WebAppResult>;
