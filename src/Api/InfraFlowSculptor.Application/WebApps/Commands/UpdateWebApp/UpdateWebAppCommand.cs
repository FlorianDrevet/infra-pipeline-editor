using InfraFlowSculptor.Application.Common.Interfaces;
using ErrorOr;
using InfraFlowSculptor.Application.WebApps.Common;
using InfraFlowSculptor.Domain.Common.BaseModels.ValueObjects;
using InfraFlowSculptor.Domain.Common.ValueObjects;

namespace InfraFlowSculptor.Application.WebApps.Commands.UpdateWebApp;

/// <summary>Command to update an existing Web App.</summary>
public record UpdateWebAppCommand(
    AzureResourceId Id,
    Name Name,
    Location Location,
    Guid AppServicePlanId,
    string RuntimeStack,
    string RuntimeVersion,
    bool AlwaysOn,
    bool HttpsOnly,
    string DeploymentMode,
    Guid? ContainerRegistryId,
    string? DockerImageName,
    string? DockerfilePath = null,
    string? SourceCodePath = null,
    string? BuildCommand = null,
    string? ApplicationName = null,
    IReadOnlyList<WebAppEnvironmentConfigData>? EnvironmentSettings = null
) : ICommand<WebAppResult>;
